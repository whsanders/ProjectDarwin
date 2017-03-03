using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenomeTester : MonoBehaviour {

    public Genome genome;

	// Use this for initialization
	void Start () {
        genome = new Genome(10);
        for (int i = 0; i < 10; i++)
        {
            genome[i] = i / 10.0f;
        }
        Debug.Log("genome[4]=" + genome[4]);
        Debug.Log("genome.Read() -> " + WriteArray(genome.Read()));
        Debug.Log("Randomizing...");
        genome.Randomize();
        Debug.Log("New genome: " + WriteArray(genome.Read()));
        Debug.Log("Randomizing...");
        genome.Randomize();
        Debug.Log("New genome: " + WriteArray(genome.Read()));
        Debug.Log("Randomizing...");
        genome.Randomize();
        Debug.Log("New genome: " + WriteArray(genome.Read()));

        int seed = 12345;
        Debug.Log("Randomizing with seed \"" + seed + "\"...");
        genome.Randomize(seed);
        Debug.Log("New genome: " + WriteArray(genome.Read()));
        Debug.Log("Randomizing with seed \"" + seed + "\"...");
        genome.Randomize(seed);
        Debug.Log("New genome: " + WriteArray(genome.Read()));
        Debug.Log("Randomizing with seed \"" + seed + "\"...");
        genome.Randomize(seed);
        Debug.Log("New genome: " + WriteArray(genome.Read()));

        GenomeSegment firstThree = new GenomeSegment();
        firstThree.start = 0;
        firstThree.stop = 2;
        Debug.Log("firstThree indices: " + WriteArray(firstThree.ResolveSegmentIndices(genome)));

        GenomeSegment loopEndTo5 = new GenomeSegment();
        loopEndTo5.start = int.MaxValue;
        loopEndTo5.stop = 5;
        Debug.Log("loopEndTo5 indices: " + WriteArray(loopEndTo5.ResolveSegmentIndices(genome)));

        GenomeSegment defaultSegment = GenomeSegment.GetDefaultSegment();
        Debug.Log("defaultSegment indices: " + WriteArray(defaultSegment.ResolveSegmentIndices(genome)));

        GenomeSection defaultSection = GenomeSection.GetDefaultSection();
        Debug.Log("defaultSection indices: " + WriteArray(defaultSection.ResolveSectionIndices(genome)));
        Debug.Log("defaultSection: " + WriteArray(defaultSection.ResolveSection(genome)));

        GenomeSection complexSection = new GenomeSection() { segments = new GenomeSegment[] { firstThree, loopEndTo5, defaultSegment } };
        Debug.Log("complexSection length: " + complexSection.CalculateSectionLength(genome));
        Debug.Log("complexSection indices: " + WriteArray(complexSection.ResolveSectionIndices(genome)));
        Debug.Log("complexSection: " + WriteArray(complexSection.ResolveSection(genome)));

        GenomeReader reader = new GenomeReader(genome, new GenomeSection() { segments = new GenomeSegment[] { loopEndTo5 } });
        Debug.Log("Reading genome: " + WriteArray(genome.Read()));
        Debug.Log("Has next " + reader.HasNext() + ", value " + reader.ReadNext() + ", Has next " + reader.HasNext() + ", value " + reader.ReadNext() + "");
        genome.Randomize();
        Debug.Log("Randomizing mid-read, new genome: " + WriteArray(genome.Read()));
        Debug.Log("Has next " + reader.HasNext() + ", value " + reader.ReadNext() + ", Has next " + reader.HasNext() + ", value " + reader.ReadNext() + "");
        Debug.Log("Has next " + reader.HasNext() + ", value " + reader.ReadNext());
        Debug.Log("Has next " + reader.HasNext() + ", value " + reader.ReadNext());
        Debug.Log("Has next " + reader.HasNext() + ", value " + reader.ReadNext());
        Debug.Log("Has next " + reader.HasNext());
    }

    private string WriteArray<T>(T[] arr)
    {
        if (arr == null) return "<null>";
        string list = "";
        if (arr.Length > 0)
        {
            list = arr[0].ToString();
            for (int i = 1; i < arr.Length; i++)
            {
                list += ", " + arr[i].ToString();
            }
        }
        return (typeof(T)).FullName + "[" + arr.Length + "] {" + list + "}";
    }
}
