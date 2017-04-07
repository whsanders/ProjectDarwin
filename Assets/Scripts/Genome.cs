using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Genome {

    private float[] genome;
    private float[] sigma;

    public Genome(int length)
    {
        this.genome = new float[length];
        this.sigma = new float[length];
    }

    public void Randomize()
    {
        Randomize(System.DateTime.Now.GetHashCode());
    }

    public void Randomize(int seed)
    {
        Random.InitState(seed);
        for (int i = 0; i < genome.Length; i++)
        {
            genome[i] = Random.Range(0.0f, 1.0f);
            sigma[i] = Random.Range(0.0f, 1.0f);
        }
    }

    public static Genome Recombine(Genome mother, Genome father)
    {
        return Recombine(mother, father, RecombinationMethod.Discrete);
    }

    public static Genome Recombine(Genome mother, Genome father, RecombinationMethod method)
    {
        RecombinationMethod sigmaMethod = RecombinationMethod.Intermediate;
        if (method == RecombinationMethod.Crossover)
        {
            // Do we want to default to something else for sigmas here?
        }
        return Recombine(mother, father, method, sigmaMethod);
    }

    public static Genome Recombine(Genome mother, Genome father, RecombinationMethod method, RecombinationMethod sigmaMethod)
    {
        // This could get a little messy; to keep things simpler we pick a length for the child genome and calculate a 
        //     crossover point (which may never be used) up front, then take a best effort approach within them. A side 
        //     effect of this is that any selection bias between mother and father in the upstream logic could lead to 
        //     uneven results here. This may be desired in some cases; if so, see comments in the individual switch cases 
        //     below for the precise behaviors.
        int commonLength = Mathf.Min(mother.Length, father.Length);
        int childLength = method == RecombinationMethod.Crossover ? father.Length : mother.Length;
        int crossoverPoint = Random.Range(0, Mathf.Min(mother.Length, father.Length));

        Genome child = new Genome(childLength);
        for (int i = 0; i < child.Length; i++)
        {
            switch (method)
            {
                case RecombinationMethod.Crossover:
                    // Crossover is always from mother to father, never the reverse. However, crossover can happen at 0, 
                    //     meaning this will (rarely) produce a direct copy of the father.
                    if (i < crossoverPoint)
                    {
                        child.genome[i] = mother.genome[i];
                    } else
                    {
                        child.genome[i] = father.genome[i];
                    }
                    break;
                case RecombinationMethod.Discrete:
                    // Discrete takes one or the other value randomly from the parent(s) which have values at the current 
                    //     position. Child's length is always equal to mother's length, so we only need to check against 
                    //     the father.
                    if (i < father.Length)
                    {
                        child.genome[i] = Random.Range(0, 2) == 0 ? mother.genome[i] : father.genome[i];
                    } else
                    {
                        child.genome[i] = mother[i];
                    }
                    break;
                case RecombinationMethod.Fixed:
                    // Fixed produces a direct copy of the mother.
                    child.genome[i] = mother.genome[i];
                    break;
                case RecombinationMethod.Intermediate:
                    // Intermediate takes a value between the two parent values; if only one parent has a value at the 
                    //     current position, that value is used instead. Child's length is always equal to mother's, so 
                    //     only the father must be checked.
                    if (i < father.Length)
                    {
                        float smaller = Mathf.Min(mother.genome[i], father.genome[i]);
                        float larger = Mathf.Max(mother.genome[i], father.genome[i]);
                        if (smaller == larger)
                        {
                            child.genome[i] = smaller;
                        } else
                        {
                            child.genome[i] = Random.Range(smaller, larger);
                        }
                    } else
                    {
                        child.genome[i] = mother.genome[i];
                    }
                    break;
                default:
                    throw new System.InvalidOperationException("RecombinationMethod not supported: " + method.ToString());
            }
            switch (sigmaMethod)
            {
                // These behave exactly the same as above, but we have to be a little more careful with length checking, 
                //     since child length is based on the primary recombination method, not this one.
                case RecombinationMethod.Crossover:
                    if (i < crossoverPoint)
                    {
                        child.sigma[i] = mother.sigma[i];
                    } else if (i < father.Length)
                    {
                        child.sigma[i] = father.sigma[i];
                    } else
                    {
                        // This is a weird case, implying we're doing crossover of the sigmas but not the genome itself, 
                        //     but hey whatever you want maaaan. Probably best to just "cross back" to the mother, here.
                        child.sigma[i] = mother.sigma[i];
                    }
                    break;
                case RecombinationMethod.Discrete:
                    float[] candidates = new float[2];
                    int count = 0;
                    if (i < mother.Length) candidates[count++] = mother.sigma[i];
                    if (i < father.Length) candidates[count++] = father.sigma[i];
                    child.sigma[i] = candidates[Random.Range(0, count)];
                    break;
                case RecombinationMethod.Fixed:
                    if (i < mother.Length)
                    {
                        child.sigma[i] = mother.sigma[i];
                    } else
                    {
                        // A bit strange perhaps, but just use father's. If this throws an exception the upstream logic 
                        //     has a Bad Problem(tm).
                        child.sigma[i] = father.sigma[i];
                    }
                    break;
                case RecombinationMethod.Intermediate:
                    if (i < commonLength)
                    {
                        float smaller = Mathf.Min(mother.sigma[i], father.sigma[i]);
                        float larger = Mathf.Max(mother.sigma[i], father.sigma[i]);
                        if (smaller == larger)
                        {
                            child.sigma[i] = smaller;
                        }
                        else
                        {
                            child.sigma[i] = Random.Range(smaller, larger);
                        }
                    }
                    else if (i < mother.Length)
                    {
                        child.sigma[i] = mother.sigma[i];
                    } else
                    {
                        child.sigma[i] = father.sigma[i];
                    }
                    break;
                default:
                    throw new System.InvalidOperationException("RecombinationMethod not supported: " + sigmaMethod.ToString());
            }
        }
        return child;
    }

    // Mutation here is performed on both the genome and strategy parameters, following Baeck, et al. (1992)
    public void Mutate()
    {
        float tau = 1.0f / Mathf.Sqrt(2.0f * this.Length);
        float commonScalingFactor = Mathf.Exp(Random.Range(-tau, tau));

        float tauPrime = 1.0f / Mathf.Sqrt(2.0f * Mathf.Sqrt(this.Length));  // Used for individual scaling factors below
        for (int i = 0; i < this.Length; i++)
        {
            float individualScalingFactor = Mathf.Exp(Random.Range(-tauPrime, tauPrime));

            // mutate strategy param
            float sigmaPrime = sigma[i] * individualScalingFactor * commonScalingFactor;
            sigmaPrime = (sigmaPrime < 0.0f ? 0.0f : (sigmaPrime > 1.0f ? 1.0f : sigmaPrime));  // Clamp to range

            // mutate gene
            float genePrime = genome[i] + Random.Range(-sigmaPrime, sigmaPrime);
            genePrime = (genePrime < 0.0f ? 0.0f : (genePrime > 1.0f ? 1.0f : genePrime));  // Clamp to range

            genome[i] = genePrime;
            sigma[i] = sigmaPrime;
        }
    }

    public float this[int position]
    {
        get
        {
            return genome[position];
        }
        set
        {
            genome[position] = value;
        }
    }

    public int Length
    {
        get
        {
            return genome.Length;
        }
    }

    public GenomeReader GetReader()
    {
        return new GenomeReader(this);
    }

    public GenomeSection ToSection()
    {
        return GenomeSection.GetDefaultSection();
    }

    public float[] Read()
    {
        return this.Read(this.ToSection());
    }

    public float[] Read(GenomeSection section)
    {
        return section.ResolveSection(this);
    }
}

public class GenomeReader
{
    private Genome genome;
    private GenomeSection section;
    private int position = 0;
    private int[] sectionIndices;

    public GenomeReader(Genome genome) : this(genome, genome.ToSection()) {
    }

    public GenomeReader(Genome genome, GenomeSection section)
    {
        this.genome = genome;
        this.section = section;
        this.StartRead();
    }

    private void StartRead()
    {
        this.sectionIndices = section.ResolveSectionIndices(genome);
        Reset();
    }

    public void Reset()
    {
        this.position = 0;
    }

    public bool HasNext()
    {
        return sectionIndices != null && position < sectionIndices.Length;
    }

    public float ReadNext()
    {
        float gene = genome[sectionIndices[position]];
        position++;
        return gene;
    }
}

public struct GenomeSegment
{
    // An abstract mapping for a segment of a genome. Segment goes from start to stop, inclusive at both ends.  If
    //   either start or stop is before (after) the bounds of the genome it is understood to "snap to" the first (last)
    //   index instead.  E.g. for a genome of length 30, a segment with a start of -100 and stop of 100 would be understood
    //   to go from 0 to 29, i.e. the full genome.  If start > stop the segment "wraps", i.e. the genome should be
    //   treated as a continuous loop.  Note that a segment ALWAYS reads at least one gene; there is no such thing
    //   as an empty or null segment.  A segment with start==stop will read that single gene (which may be one of 
    //   the ends if it is outside bounds).  One more example: in the previous example of length 30, if the segment were
    //   reversed, to have a start of 100 and a stop of -100, it would read TWO genes: first it would snap 100 to the end and
    //   read gene 29, then it would wrap and snap -100 to 0, read that gene, then halt.
    public int start;
    public int stop;

    public static GenomeSegment GetDefaultSegment()
    {
        GenomeSegment segment = new GenomeSegment();
        segment.start = int.MinValue;
        segment.stop = int.MaxValue;
        return segment;
    }

    public int CalculateSegmentLength(Genome genome)
    {
        int begin = Clamp(this.start, genome);
        int end = Clamp(this.stop, genome);
        int length = 1 + end - begin;
        if (length <= 0) length += genome.Length;  // wrap
        return length;
    }

    public int[] ResolveSegmentIndices(Genome genome)
    {
        int[] indices;
        int begin = Clamp(this.start, genome);
        int end = Clamp(this.stop, genome);
        int length = 1 + end - begin;
        if (begin <= end)
        {
            indices = new int[length];
            for (int i = begin; i <= end; i++)
            {
                indices[i - begin] = i;
            }
        } else
        {
            length += genome.Length;
            indices = new int[length];
            for (int i = begin; i < genome.Length; i++)
            {
                indices[i - begin] = i;
            }
            for (int i = 0; i <= end; i++)
            {
                indices[genome.Length - begin + i] = i;
            }
        }
        return indices;
    }

    private static int Clamp(int position, Genome genome)
    {
        return position < 0 ? 0 : position >= genome.Length ? genome.Length - 1 : position;
    }
}

public struct GenomeSection
{
    public GenomeSegment[] segments;

    public static GenomeSection GetDefaultSection()
    {
        return new GenomeSection() { segments = new GenomeSegment[] { GenomeSegment.GetDefaultSegment() } };
    }

    public int CalculateSectionLength(Genome genome)
    {
        int length = 0;
        if (segments != null)
        {
            foreach (GenomeSegment segment in this.segments)
            {
                length += segment.CalculateSegmentLength(genome);
            }
        }
        return length;
    }

    public int[] ResolveSectionIndices(Genome genome)
    {
        int[] indices = new int[this.CalculateSectionLength(genome)];
        int position = 0;
        foreach (GenomeSegment segment in this.segments)
        {
            int[] segmentIndices = segment.ResolveSegmentIndices(genome);
            segmentIndices.CopyTo(indices, position);
            position += segmentIndices.Length;
        }
        if (position != indices.Length) Debug.LogWarning("Section indices position " + position + " doesn't match length " + indices.Length + "!");
        return indices;
    }

    public float[] ResolveSection(Genome genome)
    {
        int[] indices = ResolveSectionIndices(genome);
        float[] genes = new float[indices.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            genes[i] = genome[indices[i]];
        }
        return genes;
    }
}

public enum RecombinationMethod
{
    Crossover,
    Discrete,
    Fixed,
    Intermediate
}