using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Genome {

    private float[] genome;

    public Genome(int length)
    {
        this.genome = new float[length];
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
        }
    }

    public float this[int position]
    {
        get
        {
            return genome[position];
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