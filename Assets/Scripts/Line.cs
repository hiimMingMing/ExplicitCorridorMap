using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Line : MonoBehaviour
{
    float gpi;
    int N, gtaka, ghaba;
    int i, j, k;
    float Nd, gtakad, ghabad;
    float tmpw;
    int br;
    Color colorObs, colorPnP, colorSnP, colorSnS;
    float[] gxs = new float[100];
    float[] gys = new float[100];
    float[] gxe = new float[100];
    float[] gye = new float[100];

    float[] gslope = new float[100];
    float[] gintercept = new float[100];

    float gx, gy;

    float grx1;
    float gry1;
    float grx2;
    float gry2;

    float gux1;
    float guy1;
    float gux2;
    float guy2;
    float gux3;
    float guy3;
    float gux4;
    float guy4;

    float gpboriginx;
    float gpboriginy;
    float gpbijp;
    float gpbijtheta;

    float ldi;
    float lys;
    float lx;
    float ly;
    float z1;
    float z1start;
    float z1end;

    public float rand()
    {
        float rand1;
        rand1 = UnityEngine.Random.value;
        return rand1;
    }//rand

    public void init()
    {
        colorObs = Color.white;
        colorPnP = Color.yellow;
        colorSnP = Color.red;
        colorSnS = Color.magenta;

        ghabad = 1000;
        gtakad = 500;
        ghaba = (int)ghabad;
        gtaka = (int)gtakad;
        N = 20;
        if (N == 20)
        {
            N = 2 + (int)(14 * rand());
        }

        gpi = Mathf.PI;//pi

        //s is start
        //e is end
        //l is left
        //r is right
        //s sometime is l
        //e sometime is r

        float slx;
        float sly;
        float srx;
        float sry;

        float elx;
        float ely;
        float erx;
        float ery;

        float selx;
        float sely;
        float serx;
        float sery;

        float eslx;
        float esly;
        float esrx;
        float esry;

        float dxs;
        float dxe;
        float dys;
        float dye;
        float dslope;
        float dintercept;
        float minx;
        float maxx;

        int[] xsI = new int[100];
        int[] ysI = new int[100];
        int[] xeI = new int[100];
        int[] yeI = new int[100];
        float[] s = new float[100];
        string[] sss = new string[100];


        //^Setting generators(Line segments)
        //^In this version, these line segments are set such that they don't cross each other.

        k = 0;
        while (k < N)
        {
            dxs = rand() * (ghaba - 30) + 15;
            dys = rand() * (gtaka - 30) + 15;
            dxe = rand() * (ghaba - 30) + 15;
            dye = rand() * (gtaka - 30) + 15;
            dslope = (dye - dys) / (dxe - dxs);
            dintercept = dys - dslope * dxs;
            minx = dxs;
            maxx = dxe;
            if (dxs > dxe)
            {
                minx = dxe;
                maxx = dxs;
            }

            gxs[k] = dxs;//^k's generator is defined (gxs[k],gys[k])-(gxe[k],gye[k])
                         //^g is global variables
                         //^x is x-coordinate, y is y-coordinate
                         //^s is start point, e is end point.
            gys[k] = dys;
            gxe[k] = dxe;
            gye[k] = dye;
            if (gxs[k] > gxe[k])
            {
                tmpw = gxs[k];
                gxs[k] = gxe[k];
                gxe[k] = tmpw;
                tmpw = gys[k];
                gys[k] = gye[k];
                gye[k] = tmpw;
            }
            xsI[k] = (int)(gxs[k]);
            ysI[k] = (int)(gys[k]);
            xeI[k] = (int)(gxe[k]);
            yeI[k] = (int)(gye[k]);
            Debug.DrawLine(new Vector3(xsI[k], ysI[k], 0), new Vector3(xeI[k], yeI[k], 0), colorObs);
            gslope[k] = dslope;
            gintercept[k] = dintercept;
            k++;
        }//while k<n
    }//init
    public float jou(float a, float b)
    {
        float jou1;
        jou1 = Mathf.Pow(a, b);
        return jou1;
    }//jou


    public float dis(float x1, float y1, float x2, float y2)
    {
        return Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2));
    }//dis
   
    //main function
    public void Bake()
    {
        Debug.ClearDeveloperConsole();
        init();
        
        for (i = 0; i < N - 1; i++)
        {
            for (j = i + 1; j < N; j++)
            {
                //bisector of i's left vertex and j's left vertex 
                ComputePointAndPoint(i,j,gxs[i], gys[i], gxs[j], gys[j], gxe[i], gye[i],gxe[j],gye[j]);
                //bisector of i's right vertex and j's right vertex  
                ComputePointAndPoint(i,j,gxe[i], gye[i], gxe[j], gye[j], gxs[i], gys[i], gxs[j], gys[j]);
                //bisector of i's left vertex and j's right vertex
                ComputePointAndPoint(i,j,gxs[i], gys[i], gxe[j], gye[j], gxe[i], gye[i], gxs[j], gys[j]);
                //bisector of i's right vertex and j's left vertex 
                ComputePointAndPoint(i,j,gxe[i], gye[i], gxs[j], gys[j], gxs[i], gys[i], gxe[j], gye[j]);

                //bisector of two line segments  i,j
                bisec2seg(i, j);
                ComputeSegmentAndSegment(i,j,gux1, guy1, gux2, guy2);
                ComputeSegmentAndSegment(i, j, gux3, guy3, gux4, guy4);

                //parabola bisector of line segment i and left point of segment j
                ComputeSegmentAndPoint(i, j, gxs[j], gys[j], gxe[j], gye[j]);
                //parabola bisector of line segment i and right point of segment j
                ComputeSegmentAndPoint(i, j, gxe[j], gye[j], gxs[j], gys[j]);
                //parabola bisector of line segment j and left point of segment i　
                ComputeSegmentAndPoint(j, i, gxs[i], gys[i], gxe[i], gye[i]);
                //parabola bisector of line segment j and right point of segment i　
                ComputeSegmentAndPoint(j, i, gxe[i], gye[i], gxs[i], gys[i]);
            }//j
        }//i
    }//paint main
    private void ComputePointAndPoint(int i, int j, float gxsi, float gysi, float gxsj, float gysj, float gxei, float gyei, float gxej, float gyej)
    {
        //bisector of i's left vertex and j's left vertex 
        bisec2poi(gxsi, gysi, gxsj, gysj);
        float slx = grx1;
        float sly = gry1;
        float srx = grx2;
        float sry = gry2;

        ldi = (sly - sry) / (slx - srx);
        lys = sly - ldi * slx;

        z1start = slx;
        z1end = srx;
        if (Mathf.Abs(ldi) > 1.0f)
        {
            if (ldi > 1.0f)
            {
                z1start = slx * ldi + lys;
                z1end = srx * ldi + lys;
            }
            else
            {
                z1start = srx * ldi + lys;
                z1end = slx * ldi + lys;
            }
        }
        List<Vector2> points = new List<Vector2>();
        
        for (z1 = z1start; z1 < z1end; z1 = z1 + 0.5f)
        {
            if (Mathf.Abs(ldi) < 1.0f)
            {
                lx = z1;
                ly = lx * ldi + lys;
            }
            else
            {
                ly = z1;
                lx = (ly - lys) / ldi;
            }
            int cnt;
            cnt = 0;
            float disis;
            float disiseps;
            float disjseps;
            disis = dis(lx, ly, gxsi, gysi);
            disiseps = dis(lx, ly, gxsi * 0.999f + gxei * 0.001f, gysi * 0.999f + gyei * 0.001f);
            disjseps = dis(lx, ly, gxsj * 0.999f + gxej * 0.001f, gysj * 0.999f + gyej * 0.001f);
            if (disiseps < disis)
            {
                cnt++;
            }
            if (disjseps < disis)
            {
                cnt++;
            }
            if (cnt == 0)
            {
                if (cntorder(i, j, lx, ly, disis) == 0)
                {
                    points.Add(new Vector2(lx, ly));
                }
            }
        }//lx
        for (int index=0;index < points.Count - 1; index++)
        {
            Debug.DrawLine(points[index], points[index + 1], colorPnP);
        }
        points.Clear();

    }
    private void ComputeSegmentAndSegment(int i, int j, float lsegcsx1, float lsegcsy1, float lsegcex1, float lsegcey1)
    {
        ldi = (lsegcsy1 - lsegcey1) / (lsegcsx1 - lsegcex1);//slope of (gux1,guy1)-(gux2,guy2)　(gux1,guy1)-(gux2,guy2)
        lys = lsegcsy1 - ldi * lsegcsx1;//intercept of (gux1,guy1)-(gux2,guy2)　(gux1,guy1)-(gux2,guy2)
                                        //                for(lx=lsegcsx1;lx<lsegcex1;lx=lx+0.5){
                                        //if Abs(slope)<1 then for x=right of screen to left of screen, Abs(slope)>=1 then for y=bottom of screen to top of screen
        z1start = lsegcsx1;
        z1end = lsegcex1;
        if (Mathf.Abs(ldi) > 1.0f)
        {
            if (ldi > 1.0f)
            {
                z1start = lsegcsx1 * ldi + lys;
                z1end = lsegcex1 * ldi + lys;
            }
            else
            {
                z1start = lsegcex1 * ldi + lys;
                z1end = lsegcsx1 * ldi + lys;
            }
        }
        ldi = (lsegcsy1 - lsegcey1) / (lsegcsx1 - lsegcex1);
        List<Vector2> points = new List<Vector2>();

        for (z1 = z1start; z1 < z1end; z1 = z1 + 0.5f)
        {
            if (Mathf.Abs(ldi) < 1.0f)
            {
                lx = z1;
                ly = lx * ldi + lys;
            }
            else
            {
                ly = z1;
                lx = (ly - lys) / ldi;
            }
            //                    ly=lx*ldi+lys;
            int cnt;
            cnt = 0;
            float dii;
            float ysi;
            float intersectionxi;

            dii = -1.0f / gslope[i];//slope, dii of perpendicular line from (lx,ly) to line segment i (lx,ly)
            ysi = ly - dii * lx;//intercept, ysi of perpendicular line from (lx,ly) to line segment i (lx,ly)
            intersectionxi = (ysi - gintercept[i]) / (gslope[i] - dii);//x coordinate of intersection of perpendicular line from (lx,ly) to line segment i and line segment i  
                                                                       //if intersection is outside of line segment i then don't draw bisector
            if (intersectionxi < gxs[i] || intersectionxi > gxe[i])
            {
                cnt++;
            }//intersectionxi<gxs[i] or intersectionxi>gxe[i]
            float dij;
            float ysj;
            float intersectionxj;
            dij = -1.0f / gslope[j];//slope, dij of perpendicular line from (lx,ly) to line segment j (lx,ly)
            ysj = ly - dij * lx;//intercept, ysj of perpendicular line from (lx,ly) to line segment j (lx,ly)
            intersectionxj = (ysj - gintercept[j]) / (gslope[j] - dij);//x coordinate of intersection of perpendicular line from (lx,ly) to line segment j and line segment j 
                                                                       //if intersection is outside of line segment j then don't draw bisector
            if (intersectionxj < gxs[j] || intersectionxj > gxe[j])
            {
                cnt++;
            }
            //cnt==0 means   perpendicular line from (lx,ly) to i crosses inside line segment i and perpendicular line from (lx,ly) to j crosses inside line segment j
            if (cnt == 0)
            {
                float disijc;
                disijc = Mathf.Abs(gslope[i] * lx - ly + gintercept[i]) / jou(jou(gslope[i], 2.0f) + 1.0f, 0.5f);//distance from (lx,ly) to segment i(same to sengemnt j)  
                                                                                                                 //compute distance to k. If distance to i is less than any other distance to k then plot (lx,ly) as bisector of i and j
                if (cntorder(i, j, lx, ly, disijc) == 0)
                {
                    points.Add(new Vector2(lx, ly));
                }
            }
        }//lx
        for (int index = 0; index < points.Count - 1; index++)
        {
            Debug.DrawLine(points[index], points[index + 1], colorSnS);
        }
        points.Clear();
    }
    private void ComputeSegmentAndPoint(int i, int j, float gxsj, float gysj, float gxej, float gyej)
    {
        float lpboriginx;
        float lpboriginy;
        float x;
        float y;
        float xrmp;
        float yrmp;
        float ystart;
        float yend;

        float disjs;
        float disje;
        int cnt;

        float lpbijsp;
        float lpbijstheta;

        //parabola bisector of line segment i and left point of segment j
        bisecsegpoi(i, j, gxsj, gysj);
        lpbijsp = gpbijp;
        lpbijstheta = gpi - gpbijtheta;
        if (gxsj > gpboriginx)
        {
            lpbijstheta = gpi * 2.0f - gpbijtheta;
        }
        lpboriginx = gpboriginx;
        lpboriginy = gpboriginy;
        float yisrm;
        float yierm;
        yisrm = Mathf.Sin(lpbijstheta) * (gxs[i] - lpboriginx) + Mathf.Cos(lpbijstheta) * (gys[i] - lpboriginy);
        yierm = Mathf.Sin(lpbijstheta) * (gxe[i] - lpboriginx) + Mathf.Cos(lpbijstheta) * (gye[i] - lpboriginy);
        ystart = yisrm;
        yend = yierm;
        if (yisrm > yierm)
        {
            ystart = yierm;
            yend = yisrm;
        }
        lpboriginx = gpboriginx;
        lpboriginy = gpboriginy;
        List<Vector2> points = new List<Vector2>();

        for (y = ystart; y < yend; y = y + 0.1f)
        {
            x = jou(y, 2.0f) / (4.0f * lpbijsp);
            xrmp = lpboriginx + Mathf.Cos(-lpbijstheta) * x - Mathf.Sin(-lpbijstheta) * y;
            yrmp = lpboriginy + Mathf.Sin(-lpbijstheta) * x + Mathf.Cos(-lpbijstheta) * y;
            cnt = 0;
            disjs = dis(xrmp, yrmp, gxsj, gysj);
            disje = dis(xrmp, yrmp, gxej, gyej);
            if (disje < disjs)
            {
                cnt++;
            }
            else
            {
                float disjseps;
                disjseps = dis(xrmp, yrmp, gxsj * 0.999f + gxej * 0.001f, gysj * 0.999f + gyej * 0.001f);
                if (disjseps < disjs)
                {
                    cnt++;
                }
            }
            lpboriginx = gpboriginx;
            lpboriginy = gpboriginy;
            if (cnt == 0)
            {
                if (cntorder(i, j, xrmp, yrmp, disjs) == 0)
                {
                    points.Add(new Vector2(xrmp, yrmp));
                }
            }//cnt==0
        }//y
        for (int index = 0; index < points.Count - 1; index++)
        {
            Debug.DrawLine(points[index], points[index + 1], colorSnP);
        }
        points.Clear();
    }
    //bisectors of two points 
    void bisec2poi(float tx1, float ty1, float tx2, float ty2)
    {
        float ssdi;
        float ssdi2;
        float ssys;
        float sscx;
        float sscy;

        ssdi = (ty1 - ty2) / (tx1 - tx2);
        ssdi2 = -1.0f / ssdi;
        sscx = (tx1 + tx2) / 2.0f;
        sscy = (ty1 + ty2) / 2.0f;
        ssys = sscy - ssdi2 * sscx;

        grx1 = 0.0f;
        gry1 = ssys;
        if (ssys < 0.0f)
        {
            grx1 = -ssys / ssdi2;
            gry1 = 0.0f;
        }
        if (ssys > gtakad)
        {
            grx1 = (gtakad - ssys) / ssdi2;
            gry1 = gtakad;
        }
        grx2 = ghabad;
        gry2 = ssdi2 * ghabad + ssys;
        if (gry2 < 0.0f)
        {
            grx2 = -ssys / ssdi2;
            gry2 = 0.0f;
        }
        if (ssdi2 * ghabad + ssys > gtakad)
        {
            grx2 = (gtakad - ssys) / ssdi2;
            gry2 = gtakad;
        }
    }//bisec2poi

    //2 bisectors of line segment i and line segment j　
    void bisec2seg(int pari, int parj)
    {


        float lcx1;
        float lcy1;
        float lcx2;
        float lcy2;

        float lslope;
        float lintercept;

        float lthetai;
        float lthetaj;
        float lctheta;
        float lcslope;
        float lcys;


        lcx1 = 100.0f;
        lcy1 = 10.0f;

        //(lcx1,lcy1) is intersection of two line segments. 
        lcx1 = (gintercept[pari] - gintercept[parj]) / (gslope[parj] - gslope[pari]);
        lcy1 = gslope[pari] * lcx1 + gintercept[pari];

        lthetai = Mathf.Atan(gslope[pari]);//angletheta of i  
        lthetaj = Mathf.Atan(gslope[parj]);//angletheta of j  
        lctheta = (lthetai + lthetaj) / 2.0f;//angle of bisector of an angle  
        lcslope = Mathf.Tan(lctheta);//slope of bisector of an angle  
        lcys = lcy1 - lcslope * lcx1;//intercept of bisector of an angle  

        //Consider two bisectors of an angle for line segments i and j.
        //One bisector of an angles is expressed (gux1,guy1)-(gux2,guy2)
        //The other is expressed (gux3,guy3)-(gux4,guy4)
        gux1 = 0.0f;//(gux1,guy1) is left edge point of the one bisector of an angle  
        guy1 = lcys;
        if (lcys < 0.0f)
        {
            gux1 = -lcys / lcslope;
            guy1 = 0.0f;
        }
        if (lcys > gtakad)
        {
            gux1 = (gtakad - lcys) / lcslope;
            guy1 = gtakad;
        }
        gux2 = ghabad;//(gux2,guy2) is right edge point of the one bisector of an angle  
        guy2 = lcslope * ghabad + lcys;
        if (guy2 < 0.0f)
        {
            gux2 = -lcys / lcslope;
            guy2 = 0.0f;
        }
        if (lcslope * ghabad + lcys > gtakad)
        {
            gux2 = (gtakad - lcys) / lcslope;
            guy2 = gtakad;
        }

        lthetaj = Mathf.Atan(gslope[parj]);
        if (lthetai < lthetaj)
        {
            lthetai = lthetai + gpi;
        }
        else
        {
            lthetaj = lthetaj + gpi;
        }
        lctheta = (lthetai + lthetaj) / 2.0f;
        lcslope = Mathf.Tan(lctheta);
        lcys = lcy1 - lcslope * lcx1;

        gux3 = 0.0f;//(gux3,guy3) is left edge point of the other bisector of an angle  
        guy3 = lcys;
        if (lcys < 0.0f)
        {
            gux3 = -lcys / lcslope;
            guy3 = 0.0f;
        }
        if (lcys > gtakad)
        {
            gux3 = (gtakad - lcys) / lcslope;
            guy3 = gtakad;
        }
        gux4 = ghabad;//(gux4,guy4) is right edge point of the other bisector of an angle  はもう一方の角の二等分線の右端
        guy4 = lcslope * ghabad + lcys;
        if (guy4 < 0.0f)
        {
            gux4 = -lcys / lcslope;
            guy4 = 0.0f;
        }
        if (lcslope * ghabad + lcys > gtakad)
        {
            gux4 = (gtakad - lcys) / lcslope;
            guy4 = gtakad;
        }

    }//bisec2poi

    //parabola bisector of line segment i and a point of segment j
    void bisecsegpoi(int pari, int parj, float gxs, float gys)
    {
        float lthetaj;
        float dis;
        float lslopej;
        float linterceptj;
        float lp;
        float lintersectionx;
        float lintersectiony;
        float lcx;
        float lcy;

        //When the line (directrix) is x=-lp and the point (focus) is (lp,0) then the parabola line is y^2=4lpx.

        //At first, lp is set to the half of distance between segment i and edge point of j.
        //Rotate segment i and edge point of j by the angle of segment i, that is, make line segment i x=lp. Next, move segment i and edge point of j such that the middlepoint of segment i and edge point of j become origin point, (0,0). From these actions, we can draw y^2=4lpx for segment i and edge point of j.
        //After exit this subroutine, rotate back and move back. then you can draw exact parabola.

        //distance between line i and point( point of j)
        dis = Mathf.Abs(gslope[pari] * gxs - gys + gintercept[pari]) / jou(jou(gslope[pari], 2.0f) + 1.0f, 0.5f);
        lp = dis / 2.0f;
        lslopej = -1.0f / gslope[pari];
        linterceptj = gys - lslopej * gxs;
        lintersectionx = (gintercept[pari] - linterceptj) / (lslopej - gslope[pari]);
        lintersectiony = lintersectionx * lslopej + linterceptj;
        lcx = (gxs + lintersectionx) / 2.0f;
        lcy = (gys + lintersectiony) / 2.0f;
        lthetaj = Mathf.Atan(lslopej);

        //(gpboriginx,gpboriginy) is origin point for segment i and edge point of j for y^2=4lpx.
        gpboriginx = lcx;
        gpboriginy = lcy;
        //gpbijp is lp when bisector is y^2=4lpx for line segment i and edge point of j. 
        gpbijp = lp;
        //gpbijtheta is angle of rotation when bisector is y^2=4lpx for line segment i and edge point of j. 
        gpbijtheta = lthetaj;

    }//bisecsegpois

 

    public int cntorder(int si, int sj, float sx, float sy, float sd)
    {
        int lcnt;
        int l;
        lcnt = 0;
        for (l = 0; l < N; l++)
        {
            if (l != si && l != sj)
            {
                float disl;
                disl = Mathf.Abs(gslope[l] * sx - sy + gintercept[l]) / jou(jou(gslope[l], 2.0f) + 1.0f, 0.5f);
                if (disl < sd)
                {
                    float dil;
                    float ysl;
                    float intersectionxl;
                    dil = -1.0f / gslope[l];
                    ysl = sy - dil * sx;
                    intersectionxl = (ysl - gintercept[l]) / (gslope[l] - dil);
                    if (intersectionxl < gxs[l] || intersectionxl > gxe[l])
                    {
                        float disls;
                        disls = dis(sx, sy, gxs[l], gys[l]);
                        if (disls < sd)
                        {
                            lcnt++;
                        }
                        else
                        {
                            float disle;
                            disle = dis(sx, sy, gxe[l], gye[l]);
                            if (disle < sd)
                            {
                                lcnt++;
                            }
                        }
                    }
                    else
                    {
                        lcnt++;
                    }
                    if (lcnt > 0)
                    {
                        break;
                    }
                }//disl<sd
            }//l!=si && l!=sj
        }//l
        return lcnt;
    }//cntorder

}
