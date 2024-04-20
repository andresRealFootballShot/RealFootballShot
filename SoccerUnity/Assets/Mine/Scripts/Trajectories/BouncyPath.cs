using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyPath : Path
{
    //Key=momento inicial del path
    SortedDictionary<float,Path> paths;
    float lastPathT;
    float groundY,bounciness,slidingFriction,offsetGroundY;
    public int aux;
    Path pathInGround;
    public string info;
    public BouncyPath(Path path0,Path pathInGround,float _groundY,float offsetGroundY, float _bounciness, float _slidingFriction) : base(path0.Pos0, path0.V0, path0.t0)
    {
        paths = new SortedDictionary<float, Path>();
        paths.Add(0,path0);
        groundY = _groundY;
        lastPathT = 0;
        bounciness = _bounciness;
        slidingFriction = _slidingFriction;
        this.offsetGroundY = offsetGroundY;
        this.pathInGround = pathInGround;
    }
    public override Vector3 getPositionAtTime(float t)
    {
        Vector3 pos;
        Path path = getPath(t);
        pos = path.getPositionAtTime(t - path.t0);
        
        return pos;
    }
    Path getPath(float t)
    {
        float pathTime;
        Path path = searchPathInList(t, out pathTime);
        
        return getPath(t,path);
    }
    public void buildPath(float t)
    {
        getPath(t);
    }
    Path getPath(float t,Path path)
    {
        float pathTime = path.t0;
        Vector3 pos = path.getPositionAtTime(t - pathTime);
        //Debug.Log("b " + path.ToString());

        //print(t + " " + pathTime + " " + pos.y + " ground=" + (groundY + offsetGroundY));
        if (pos.y < groundY - offsetGroundY)
        {
            List<float> list = path.timeToReachY(groundY, 1);
            
            if (list.Count > 0)
            {
                float ty = list[0];
                Vector3 newPos = path.getPositionAtTime(ty);
                Vector3 newVelocity = path.getVelocityAtTime(ty);
                //newVelocity = MyFunctions.setYToVector3(newVelocity, getBounceVy(newVelocity.y));
                //
                if (ty > lastPathT)
                {
                    lastPathT = ty + pathTime;
                }
                Path newPath;
                
                if (Mathf.Abs(newVelocity.y) < 0.1f)
                {
                    //Debug.Log("a t="+t+" v="+path.V0.magnitude+" newv="+ newVelocity.magnitude);
                    //print("eo "+newVelocity);
                    newPath = pathInGround.newPath(newPos, MyFunctions.setYToVector3(newVelocity, 0), ty + pathTime);
                }
                else
                {
                    
                    newVelocity = getBounceV(newVelocity);
                    newPath = path.newPath(newPos, newVelocity, ty + pathTime);
                    //print(t + " newVelocity=" + newVelocity);
                }

                //Debug.Log("b="+" | "+pos+" | " + newPos+" | "+ newVelocity+" | "+ty+" | "+ (t - ty)+ " | (t- pathTime)="+ (t - pathTime));
                if (!paths.ContainsKey(ty + pathTime))
                {
                    paths.Add(ty + pathTime, newPath);
                }
                else
                {
                    paths[ty + pathTime] = newPath;
                }
                aux++;
                if (aux < 10)
                {
                    return getPath(t,newPath);
                }
                else
                {
                }
                return newPath;
            }
            else
            {
                Vector3 newPos = MyFunctions.setYToVector3(pos, groundY);
                Vector3 newVelocity = path.getVelocityAtTime(t - pathTime);
                Path newPath;
                if (Mathf.Abs(newVelocity.y) < 0.1f)
                {
                    //Debug.Log(t+" v "+ newVelocity.magnitude);
                    newPath = pathInGround.newPath(newPos, MyFunctions.setYToVector3(newVelocity, 0), t);
                }
                else
                {
                    newVelocity = getBounceV(newVelocity);
                    newVelocity = MyFunctions.setYToVector3(newVelocity, 0);
                    newPath = path.newPath(newPos, newVelocity, t);
                }
                if (!paths.ContainsKey(t))
                {
                    paths.Add(t, newPath);
                }
                else
                {
                    paths[t] = newPath;
                }
                return newPath;
            }
        }
        return path;
    }
    void print(string info)
    {
        if (this.info == "perfectPass")
        {
            Debug.Log(info);
        }
    }
    public override Vector3 getVelocityAtTime(float t)
    {
        Path path = getPath(t);
        float pathTime0 = path.t0;
        Vector3 velocity = path.getVelocityAtTime(t - pathTime0);
        //Debug.Log("z | t=" + t + " | pathTime0=" + pathTime0 + " | v=" + velocity);
        return velocity;
    }
    public override Vector3 getVelocityAtTime(float t,out Path path)
    {
        path = getPath(t);
        float pathTime0 = path.t0;
        Vector3 velocity = path.getVelocityAtTime(t - pathTime0);
        //Debug.Log("z | t=" + t + " | pathTime0=" + pathTime0 + " | v=" + velocity);
        return velocity;
    }
    Path searchPathInList(float t, out float pathTime0)
    {
        Path path = paths[0];
        pathTime0 = 0;
        foreach (var item in paths)
        {
            //Debug.Log("searchPathInList " + item.Key+ " "+item.Value.ToString());
            if (item.Key < t)
            {
                path = item.Value;
                pathTime0 = item.Key;
            }
        }
        return path;
    }
    float getBounceVy(float inputVy)
    {
        return -inputVy * bounciness;
    }
    Vector3 getBounceV(Vector3 inputV)
    {
        Vector3 outputV = new Vector3(getBounceVx(inputV.x, inputV.y), Mathf.Abs(inputV.y) * bounciness, getBounceVx(inputV.z, inputV.y));
        return outputV;
    }
    float getBounceVx(float inputVx,float inputVy)
    {
        //return inputVx - (slidingFriction*(1+bounciness)* inputVy);
        return inputVx - (slidingFriction * (1 - bounciness) * inputVx);
    }
    public override Path newPath(Vector3 _pos0, Vector3 _V0, float _t0)
    {
        return new BouncyPath(this, pathInGround, groundY,offsetGroundY,bounciness,slidingFriction);
    }
    public override List<float> getTimeOfVelocity(float yd)
    {
        Path path = paths[lastPathT];
        return path.getTimeOfVelocity(yd);
    }
    /*
    public override Vector3 getVelocityAtTime(float t)
    {
        float pathTime0;
        Path path = getPath(t, out pathTime0);
        Vector3 pos = path.getPositionAtTime(t - pathTime0);
        if(pos.y < groundY)
        {
            Debug.Log("x");
            getPositionAtTime(t);
            path = getPath(t, out pathTime0);
        }
        Vector3 velocity = path.getVelocityAtTime(t - pathTime0);
        Debug.Log("z Pos=" + pos + " | t=" + t + " | pathTime0=" + pathTime0+ " | v=" + velocity);
        return velocity;
    }*/
    /*ublic override Vector3 getPositionAtTime(float t)
    {
        Vector3 pos;
        float pathTime;
        Path path = searchPathInList(t, out pathTime);
        pos = path.getPositionAtTime(t - pathTime);
        if (pos.y < groundY)
        {
            List<float> list = path.timeToReachY(groundY, 1);
            if (list.Count > 0)
            {
                float ty = list[0];
                Vector3 newPos = path.getPositionAtTime(ty);
                Vector3 newVelocity = path.getVelocityAtTime(ty);
                //newVelocity = MyFunctions.setYToVector3(newVelocity, getBounceVy(newVelocity.y));
                newVelocity = getBounceV(newVelocity);
                if (ty > lastPathT)
                {
                    lastPathT = ty + pathTime;
                }

                Path newPath = path.newPath(newPos, newVelocity, 0);
                //Debug.Log("b="+" | "+pos+" | " + newPos+" | "+ newVelocity+" | "+ty+" | "+ (t - ty)+ " | (t- pathTime)="+ (t - pathTime));

                if (!paths.ContainsKey(ty + pathTime))
                {
                    paths.Add(ty + pathTime, newPath);
                }
                else
                {
                    paths[ty + pathTime] = newPath;
                }
                aux++;
                if (aux < 10)
                {
                    pos = getPositionAtTime(t);
                }
                else
                {
                }
            }
            else
            {
                pos = MyFunctions.setYToVector3(pos, groundY);
            }
        }
        return pos;
    }*/
}
