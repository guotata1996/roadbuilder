using System.Collections;
using System.Collections.Generic;

public abstract class LinearFragmentable<T> where T : LinearFragmentable<T>
{
    public abstract T Clone();

    /// <summary>
    /// f_new(0) = f_old(<paramref name="unscaled_t_start"/>)
    /// f_new(1) = f_old(<paramref name="unscaled_t_end"/>)
    /// </summary>
    public abstract void Crop(float unscaled_t_start, float unscaled_t_end);

    public List<T> MultiCut(List<float> unscaled_cutpoints_t, float length_for_close_judge = 1f)
    {
        unscaled_cutpoints_t.Sort();
        if (unscaled_cutpoints_t.Count == 0 || 
        !Algebra.isclose(unscaled_cutpoints_t[0], 0f, length_for_close_judge))
        {
            unscaled_cutpoints_t.Insert(0, 0f);
        }
        if (!Algebra.isclose(unscaled_cutpoints_t[unscaled_cutpoints_t.Count - 1], 1f, length_for_close_judge))
        {
            unscaled_cutpoints_t.Add(1f);
        }

        int fragments_count = unscaled_cutpoints_t.Count - 1;
        var rtn = new List<T>(fragments_count);
        for (int i = 0; i != fragments_count; ++i)
        {
            // Reject too close cutpoints
            if (!Algebra.isclose(unscaled_cutpoints_t[i], unscaled_cutpoints_t[i + 1], length_for_close_judge))
            {
                T added = Clone();
                added.Crop(unscaled_cutpoints_t[i], unscaled_cutpoints_t[i + 1]);
                rtn.Add(added);
            }
        }

        return rtn;
    }
}
