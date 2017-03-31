using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolvableSpawner : MonoBehaviour {

    public int spawnCount;
    public float elapsedDeltaTime;
    public float howOftenToSpawn;
    public Evolvable thingToSpawn;
    public float lastSpawnCheck;
    public float velocityVariance = 0.001f;
    public string status;
    public Vector3 lastAppliedVelocity;
    public int genomeSize = 3;
    public int reportsReceived;
    public float cumulativeFitnessReported;
    public int populationMu = 20;  // minimum parents to begin recombination
    public int populationLambda = 100;  // max size of a generation
    public bool waitForWholeGeneration = false;
    public int currentGeneration = 0;
    private Dictionary<int, List<FitnessReport>> fitnessReportsByGeneration;
    public int reportsOnFile;
    private Dictionary<int, int> genomesCreatedByGeneration;
    public float mutationRate = 0.20f;
    public float spontaneousGenerationRate = 0.05f;

    // Use this for initialization
    void Start()
    {
        spawnCount = 0;
        reportsReceived = 0;
        cumulativeFitnessReported = 0.0f;
        genomesCreatedByGeneration = new Dictionary<int, int>();
        fitnessReportsByGeneration = new Dictionary<int, List<FitnessReport>>();
        currentGeneration = 0;
        InitializeGeneration(currentGeneration);
        elapsedDeltaTime = 0.0f;
        reportsOnFile = 0;
        lastSpawnCheck = elapsedDeltaTime;
        if (velocityVariance < 0) velocityVariance = -velocityVariance;
        if (mutationRate < 0.0f) mutationRate = 0.0f;
        if (mutationRate > 1.0f) mutationRate = 1.0f;
        if (spontaneousGenerationRate < 0.0f) spontaneousGenerationRate = 0.0f;
        if (spontaneousGenerationRate > 1.0f) spontaneousGenerationRate = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        elapsedDeltaTime += dt;
        if (elapsedDeltaTime > lastSpawnCheck + howOftenToSpawn)
        {
            lastSpawnCheck = elapsedDeltaTime;
            if (CurrentGenerationCanSpawn())
            {
                spawnCount++;
                Vector3 velocity = new Vector3(
                    Random.Range(-velocityVariance, velocityVariance),
                    Random.Range(-velocityVariance, velocityVariance),
                    Random.Range(-velocityVariance, velocityVariance)
                );
                Evolvable obj = Instantiate<Evolvable>(thingToSpawn, transform.position, transform.rotation);
                lastAppliedVelocity = velocity;
                obj.GetComponent<Rigidbody>().AddForce(velocity, ForceMode.VelocityChange);
                obj.name = "Creature" + spawnCount.ToString();
                obj.genome = GenerateGenome();
                obj.generation = currentGeneration;
                obj.maxAge = howOftenToSpawn * populationMu;
                obj.reportTo = this;
            } else
            {
                CheckAndCloseGeneration();
            }
        }
        reportsOnFile = CountFitnessReports();
    }

    private void InitializeGeneration(int generation)
    {
        currentGeneration = generation;
        genomesCreatedByGeneration[currentGeneration] = 0;
        fitnessReportsByGeneration[currentGeneration] = new List<FitnessReport>();
    }

    private bool CurrentGenerationCanSpawn()
    {
        return
            genomesCreatedByGeneration[currentGeneration] < populationLambda
            && (
                currentGeneration == 0
                || (
                    waitForWholeGeneration && fitnessReportsByGeneration[currentGeneration - 1].Count >= genomesCreatedByGeneration[currentGeneration - 1]
                    || !waitForWholeGeneration && fitnessReportsByGeneration[currentGeneration - 1].Count >= populationMu
                )
            );
    }

    private void CheckAndCloseGeneration()
    {
        if (genomesCreatedByGeneration[currentGeneration] >= populationLambda)
        {
            InitializeGeneration(currentGeneration + 1);
        }
    }

    private Genome GenerateGenome()
    {
        Genome genome;

        if (currentGeneration == 0 || Random.Range(0.0f, 1.0f) <= spontaneousGenerationRate)
        {
            genome = new Genome(genomeSize);
            genome.Randomize();
        }
        else
        {
            Genome mom = SelectOneWeightedByFitness(currentGeneration - 1).genome;
            Genome dad = SelectOneWeightedByFitness(currentGeneration - 1).genome;

            genome = Genome.Recombine(mom, dad);

            if (Random.Range(0.0f, 1.0f) <= mutationRate)
            {
                genome.Mutate();
            }
        }

        genomesCreatedByGeneration[currentGeneration] = genomesCreatedByGeneration[currentGeneration] + 1;
        return genome;
    }

    private int CountFitnessReports()
    {
        int reports = 0;
        foreach (int key in fitnessReportsByGeneration.Keys)
        {
            reports += fitnessReportsByGeneration[key].Count;
        }
        return reports;
    }

    public void ReportFitness(int generation, float fitness, Genome genome)
    {
        reportsReceived++;
        cumulativeFitnessReported += fitness;
        fitnessReportsByGeneration[generation].Add(new FitnessReport { fitness = fitness, genome = genome, timestamp = elapsedDeltaTime });
    }

    private FitnessReport SelectOneWeightedByFitness(int generation)
    {
        FitnessReport[] pool = GetParentCandidatePool(generation);

        Dictionary<float, FitnessReport> topOfRange = new Dictionary<float, FitnessReport>(pool.Length);
        float tally = 0.0f;
        foreach (FitnessReport report in pool)
        {
            tally += report.fitness;
            topOfRange[tally] = report;
        }
        float selection = Random.Range(0.0f, tally);
        foreach (KeyValuePair<float, FitnessReport> candidate in topOfRange)
        {
            if (selection <= candidate.Key) return candidate.Value;
        }
        // we shouldn't get here, but if we do, always returning the top one has a good chance of causing an obvious problem
        return pool[0];
    }

    private FitnessReport[] GetParentCandidatePool(int generation)
    {
        if (waitForWholeGeneration)
        {
            return GetGenerationReportsSorted(generation);
        } else
        {
            return GetMostFit(generation, populationMu);
        }
    }

    private FitnessReport[] GetGenerationReportsSorted(int generation)
    {
        List<FitnessReport> reports = fitnessReportsByGeneration[generation];
        reports.Sort(FitnessReport.comparer);
        return reports.ToArray();
    }

    private FitnessReport[] GetMostFit(int generation, int howMany)
    {
        FitnessReport[] all = GetGenerationReportsSorted(generation);

        FitnessReport[] top = new FitnessReport[howMany];
        int copied = 0;
        foreach (FitnessReport report in all)
        {
            if (copied >= howMany) break;
            top[copied] = report;
            copied++;
        }

        return top;
    }

    class FitnessReport
    {
        public float fitness;
        public Genome genome;
        public float timestamp;
        public static Comparer comparer = new Comparer();

        public class Comparer : IComparer<FitnessReport>
        {
            public int Compare(FitnessReport x, FitnessReport y)
            {
                if (x.fitness > y.fitness) return -1;
                if (x.fitness < y.fitness) return 1;
                return 0;
            }
        }
    }
}
