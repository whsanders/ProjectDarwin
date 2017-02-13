using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolvableSpawner : MonoBehaviour {

    public int spawnCount;
    public float elapsedDeltaTime;
    public float howOftenToSpawn;
    public Evolvable thingToSpawn;
    public float lastSpawnTime;
    public float velocityVariance = 0.001f;
    public string status;
    public Vector3 lastAppliedVelocity;
    public int genomeSize = 8;
    public int reportsReceived;
    public float cumulativeFitnessReported;
    public int populationSize = 15;
    private List<FitnessReport> fitnessReports;
    public float fitnessReportExpiry;
    public int reportsOnFile;
    public int randomGenomesCreated;
    public int derivedGenomesCreated;
    public float mutationPercent = 5.0f;

    // Use this for initialization
    void Start()
    {
        spawnCount = 0;
        reportsReceived = 0;
        cumulativeFitnessReported = 0.0f;
        randomGenomesCreated = 0;
        derivedGenomesCreated = 0;
        elapsedDeltaTime = 0.0f;
        reportsOnFile = 0;
        lastSpawnTime = elapsedDeltaTime;
        if (velocityVariance < 0) velocityVariance = -velocityVariance;
        fitnessReports = new List<FitnessReport>();
        if (mutationPercent < 0.0f) mutationPercent = 0.0f;
        if (mutationPercent > 100.0f) mutationPercent = 100.0f;
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        elapsedDeltaTime += dt;
        if (elapsedDeltaTime > lastSpawnTime + howOftenToSpawn)
        {
            spawnCount++;
            lastSpawnTime = elapsedDeltaTime;
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
            obj.reportTo = this;
        }
        reportsOnFile = fitnessReports.Count;
    }

    private float[] GenerateGenome()
    {
        float[] genome = new float[genomeSize];
        if (fitnessReports.Count < populationSize)
        {
            randomGenomesCreated++;
            for (int i = 0; i < genome.Length; i++)
            {
                genome[i] = Random.Range(-1000.0f, 1000.0f);
            }
        } else
        {
            derivedGenomesCreated++;
            float[] mom = SelectParent().genome;
            float[] dad = SelectParent().genome;

            for (int i = 0; i < genome.Length; i++)
            {
                float fromMom = (i < mom.Length) ? mom[i] : Random.Range(-1000.0f, 1000.0f);
                float fromDad = (i < dad.Length) ? dad[i] : Random.Range(-1000.0f, 1000.0f);
                if (Random.Range(0, 2) == 0) genome[i] = fromMom; else genome[i] = fromDad;
                if (Random.Range(0.0f, 100.0f) <= mutationPercent)
                {
                    genome[i] = Random.Range(-1000.0f, 1000.0f);
                }
            }
        }
        return genome;
    }

    public void ReportFitness(float fitness, float[] genome)
    {
        reportsReceived++;
        cumulativeFitnessReported += fitness;
        fitnessReports.Add(new FitnessReport { fitness = fitness, genome = genome, timestamp = elapsedDeltaTime });
    }

    private FitnessReport SelectParent()
    {
        FitnessReport[] pool = GetMostFit(populationSize);

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

    private FitnessReport[] GetMostFit(int howMany)
    {
        fitnessReports.Sort(FitnessReport.comparer);
        FitnessReport[] top = new FitnessReport[howMany];
        List<FitnessReport> toDelete = new List<FitnessReport>();
        int copied = 0;
        foreach (FitnessReport report in fitnessReports)
        {
            if (fitnessReportExpiry > 0.0001f && (report.timestamp + fitnessReportExpiry < elapsedDeltaTime))
            {
                toDelete.Add(report);
                continue;
            }
            if (copied >= howMany) continue;
            top[copied] = report;
            copied++;
        }
        foreach (FitnessReport old in toDelete)
        {
            fitnessReports.Remove(old);
        }
        return top;
    }

    class FitnessReport
    {
        public float fitness;
        public float[] genome;
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
