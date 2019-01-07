using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public abstract class Curve
{
    public float z_start, z_offset;
    public float t_start, t_end;
    /*
	 * Normal of the road surface
	 */
    public abstract Vector3 upNormal(float t);

    public abstract Vector3 rightNormal(float t);

    public abstract Vector3 frontNormal(float t);

    public abstract Vector3 at(float t);

    public abstract Vector2 at_2d(float t);

    public abstract float length { get; }

    public abstract float TravelAlong(float currentParam, float distToTravel, bool zeroToOne);

    public abstract float maximumCurvature { get; }

    /* range [0, 2Pi)
     */
    public float angle_ending(bool start, float offset = 0f)
    {
        if (!Algebra.isclose(offset, 0f))
        {
            if (start)
            {
                if (offset > 0f)
                {
                    offset = paramOf(split(offset / this.length).First().at_2d(1f)).GetValueOrDefault(0f);
                }
                if (offset < 0f)
                {
                    offset = -paramOf(split(- offset / this.length).First().at_2d(1f)).GetValueOrDefault(0f);
                }
            }
            else
            {
                if (offset > 0f)
                {
                    offset = 1f - paramOf(split(1f - offset / this.length).Last().at_2d(0f)).GetValueOrDefault(0f);
                }
                if (offset < 0f)
                {
                    offset = paramOf(split(1f + offset / this.length).Last().at_2d(0f)).GetValueOrDefault(0f) - 1f;
                }
            }
        }

        if (start)
            return angle_2d(offset);
        else
        {
            float raw_angle = angle_2d(1f - offset);
            return raw_angle >= Mathf.PI ? raw_angle - Mathf.PI : raw_angle + Mathf.PI;
        }
    }


    public Vector2 at_ending_2d(bool start, float offset = 0f)
    {
        if (!Algebra.isclose(offset, 0f))
        {
            if (start)
            {
                if (offset > 0f)
                {
                    offset = paramOf(split(offset / this.length).First().at_2d(1f)).GetValueOrDefault(0f);
                }
                if (offset < 0f)
                {
                    offset = -paramOf(split(-offset / this.length).First().at_2d(1f)).GetValueOrDefault(0f);
                }
            }
            else
            {
                if (offset > 0f)
                {
                    offset = 1f - paramOf(split(1f - offset / this.length).Last().at_2d(0f)).GetValueOrDefault(0f);
                }
                if (offset < 0f)
                {
                    offset = paramOf(split(1f + offset / this.length).Last().at_2d(0f)).GetValueOrDefault(0f) - 1f;
                }
            }
        }

        if (start)
            return at_2d(offset);
        else
        {
            return at_2d(1f - offset);
        }

    }

    public Vector3 at_ending(bool start){
        //offset = offset / length;
        if (start)
            return at(0f);
        else{
            return at(1f);
        }
    }

    public abstract float angle_2d(float t);

    public Vector2 direction_ending_2d(bool start,float offset = 0f)
    {
        return Algebra.angle2dir(angle_ending(start, offset));
    }

    public Vector2 direction_2d(float t)
    {
        return new Vector2(Mathf.Cos(angle_2d(t)), Mathf.Sin(angle_2d(t)));
    }

    /* Separate the original curve to pieces <= maxLen
	 * if keep_length, 
	 * 
	 */
    public abstract List<Curve> segmentation(float maxlen);
    
    /* split curve into two parts: 1:0~t 2:t~1
     */

    public void reverse()
    {
        if (this is Line)
        {
            Vector2 tmp = ((Line)this).start;
            ((Line)this).start = ((Line)this).end;
            ((Line)this).end = tmp;
        }
        else{
            float tmp = t_start;
            t_start = t_end;
            t_end = tmp;
        }
        z_start += z_offset;
        z_offset = -z_offset;
    }

    protected float toGlobalParam(float t)
    {
        return t_start + t * (t_end - t_start);
    }

    protected float toLocalParam(float global_t)
    {
        return (global_t - t_start) / (t_end - t_start);
    }

    public List<Curve> split(float cutpoint)
    {
        cutpoint = Mathf.Clamp01(cutpoint);
        if (cutpoint >= 0.5f)
        {
            return segmentation(this.length * cutpoint);
        }
        else
        {
            this.reverse();
            List<Curve> reversedSegment = segmentation(this.length * (1 - cutpoint));
            foreach (Curve seg in reversedSegment)
            {
                seg.reverse();
            }
            reversedSegment.Reverse();
            this.reverse();
            return reversedSegment;
        }
    }

    /*resulting curve: Length = Original * (end - start)*/
    public Curve cut(float start, float end)
    {
        end = Mathf.Max(start, end);
        if (this is Line || this is Arc)
        {
            Curve rtn;
            if (start < 0f && !Algebra.isclose(start, 0f) || end > 1f && !Algebra.isclose(end, 1f))
            {
                Debug.Assert(false);
                Debug.Log("Abnormal cut, start:" + start + "end:" + end);
            }
            Curve secondAndThird = Algebra.isclose(start, 0f) ? this : split(start).Last();
            if (!Algebra.isclose(end, 1f))
            {
                Curve third = split(end).Last();
                float secondFraction = (secondAndThird.length - third.length) / secondAndThird.length;
                rtn = secondAndThird.split(secondFraction).First();
            }
            else
            {
                if (!Algebra.isclose(start, 0f))
                {
                    rtn = secondAndThird;
                }
                else
                {
                    rtn = segmentation(this.length).First();
                }
            }
            if (Mathf.Abs(rtn.at(0f).y - this.at(end).y) < Mathf.Abs(rtn.at(1f).y - this.at(end).y))
            {
                rtn.reverse();
                rtn.z_start = rtn.z_start + rtn.z_offset;
                rtn.z_offset = -rtn.z_offset;
            }
            return rtn;
        }
        else
        {
            Vector2 p1 = this.at_ending_2d(true, start * length);
            Vector2 p2 = this.at_ending_2d(false, (1f - end) * length);
            Curve rtn = deepCopy();
            rtn.t_start = toGlobalParam(paramOf(p1).Value);
            rtn.t_end = toGlobalParam(paramOf(p2).Value);
            return rtn;
        }
    }

    public Curve cutByParam(float start, float end){
        if (this is Line)
        {
            return cut(start, end);
        }
        else
        {
            Curve rtn = deepCopy();
            rtn.z_start = rtn.z_start + rtn.z_offset * start;
            rtn.z_offset = rtn.z_offset * (end - start);
            rtn.t_start = toGlobalParam(start);
            rtn.t_end = toGlobalParam(end);
            return rtn;
        }
    }

    public abstract float? paramOf(Vector2 point);

    public float? paramOf(Vector3 point){
        return paramOf(new Vector2(point.x, point.z));
    }

    public bool contains_2d(Vector2 point)
    {
        return (paramOf(point) != null && 0 <= (float)paramOf(point) && (float)paramOf(point) <= 1f);
    }

    public bool contains(Vector3 point){
        Vector2 twod_point = new Vector2(point.x, point.z);
        //Debug.Log("param of " + point + " is " + paramOf(twod_point));
        if (paramOf(twod_point) != null && 0 <= (float)paramOf(twod_point) && (float)paramOf(twod_point) <= 1f){
            //Debug.Log((this.at((float)paramOf(twod_point)) - point).magnitude.ToString("C4"));
            return Algebra.isclose(this.at((float)paramOf(twod_point)), point);
        }
        else{
            return false;
        }
    }

    public abstract override string ToString();

    public Vector3 AttouchPoint(Vector2 p){
        return this.AttouchPoint(new Vector3(p.x, 0f, p.y));
    }

    /*flatten everything, calculate attachPoint param, and return the 3-D position of this param*/
    public abstract Vector3 AttouchPoint(Vector3 p);

    public abstract Curve concat(Curve another);

    public Curve deepCopy(){
        return segmentation(maxlen: Algebra.InfLength).First();
    }

    public Curve flattened(){
        z_start = z_offset = 0f;
        return this;
    }
}