using UnityEngine;
using System.Collections;


public class Line : MonoBehaviour
{
    float gpi;
    Color gcol1, gcol2, gcol3, gcol4, gcol5, gcol6, gcol7;
    int N, gtaka, ghaba;
    int i, j, k;
    string NS, ghabaS, gtakaS;
    float Nd, gtakad, ghabad;
    float tmpw;
    int br;

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
    float gpbijsp;
    float gpbijstheta;
    float gpbijep;
    float gpbijetheta;



    public float rand()
    {
        float rand1;
        rand1 = UnityEngine.Random.value;
        return rand1;
    }//rand

    public void init()
    {
        gcol1 = Color.black;
        gcol2 = Color.yellow;
        gcol3 = Color.white;
        gcol4 = Color.green;
        gcol5 = Color.blue;
        gcol6 = Color.magenta;
        gcol7 = Color.cyan;

        ghabad = 1000;
        gtakad = 500;
        ghaba = (int)ghabad;
        gtaka = (int)gtakad;
        N = 20;
        if (N == 20)
        {
            N = 2 + (int)(14 * rand());
        }
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
        float intersectionx;
        float minx;
        float maxx;
        float minx2;
        float maxx2;

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
            Debug.DrawLine(new Vector3(xsI[k], ysI[k], 0), new Vector3(xeI[k], yeI[k], 0), Color.blue);
            gslope[k] = dslope;
            gintercept[k] = dintercept;
            k++;
        }//while k<n


        for (i = 0; i < N - 1; i++)
        {
            for (j = i + 1; j < N; j++)
            {

                //bisector of i's left vertex and j's left vertex 
                bisec2poi(gxs[i], gys[i], gxs[j], gys[j]);
                slx = grx1;
                sly = gry1;
                srx = grx2;
                sry = gry2;
                //                Debug.DrawLine((int)(slx),(int)(sly),(int)(srx),(int)(sry));
                float ldi;
                float lys;
                ldi = (sly - sry) / (slx - srx);
                lys = sly - ldi * slx;
                float lx;
                float ly;
                //                for(lx=slx;lx<srx;lx=lx+0.5){
                float z1;
                float z1start;
                float z1end;
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
                ldi = (sly - sry) / (slx - srx);
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
                    disis = dis(lx, ly, gxs[i], gys[i]);
                    disiseps = dis(lx, ly, gxs[i] * 0.999f + gxe[i] * 0.001f, gys[i] * 0.999f + gye[i] * 0.001f);
                    disjseps = dis(lx, ly, gxs[j] * 0.999f + gxe[j] * 0.001f, gys[j] * 0.999f + gye[j] * 0.001f);
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
                            Debug.DrawLine(new Vector3((int)(lx), (int)(ly), 0), new Vector3((int)(lx) + 1, (int)(ly) + 1, 0), Color.yellow);
                        }
                    }
                }//lx

                //bisector of i's right vertex and j's right vertex  
                bisec2poi(gxe[i], gye[i], gxe[j], gye[j]);
                elx = grx1;
                ely = gry1;
                erx = grx2;
                ery = gry2;
                //                Debug.DrawLine((int)(elx),(int)(ely),(int)(erx),(int)(ery));
                ldi = (ely - ery) / (elx - erx);
                lys = ely - ldi * elx;
                //                for(lx=elx;lx<erx;lx=lx+0.5){
                z1start = elx;
                z1end = erx;
                if (Mathf.Abs(ldi) > 1.0f)
                {
                    if (ldi > 1.0f)
                    {
                        z1start = elx * ldi + lys;
                        z1end = erx * ldi + lys;
                    }
                    else
                    {
                        z1start = erx * ldi + lys;
                        z1end = elx * ldi + lys;
                    }
                }
                elx = grx1;
                ely = gry1;
                erx = grx2;
                ery = gry2;
                ldi = (ely - ery) / (elx - erx);
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
                    float disie;
                    float disieeps;
                    float disjeeps;
                    disie = dis(lx, ly, gxe[i], gye[i]);
                    disieeps = dis(lx, ly, gxe[i] * 0.999f + gxs[i] * 0.001f, gye[i] * 0.999f + gys[i] * 0.001f);
                    disjeeps = dis(lx, ly, gxe[j] * 0.999f + gxs[j] * 0.001f, gye[j] * 0.999f + gys[j] * 0.001f);
                    if (disieeps < disie)
                    {
                        cnt++;
                    }
                    if (disjeeps < disie)
                    {
                        cnt++;
                    }
                    if (cnt == 0)
                    {
                        if (cntorder(i, j, lx, ly, disie) == 0)
                        {
                            Debug.DrawLine(new Vector3((int)(lx), (int)(ly), 0), new Vector3((int)(lx) + 1, (int)(ly) + 1, 0), Color.yellow);
                        }
                    }
                }//lx

                //bisector of i's left vertex and j's right vertex
                bisec2poi(gxs[i], gys[i], gxe[j], gye[j]);
                selx = grx1;
                sely = gry1;
                serx = grx2;
                sery = gry2;
                //                Debug.DrawLine((int)(selx),(int)(sely),(int)(serx),(int)(sery));
                ldi = (sely - sery) / (selx - serx);
                lys = sely - ldi * selx;
                //                for(lx=selx;lx<serx;lx=lx+0.5){
                z1start = selx;
                z1end = serx;
                if (Mathf.Abs(ldi) > 1.0f)
                {
                    if (ldi > 1.0f)
                    {
                        z1start = selx * ldi + lys;
                        z1end = serx * ldi + lys;
                    }
                    else
                    {
                        z1start = serx * ldi + lys;
                        z1end = selx * ldi + lys;
                    }
                }
                selx = grx1;
                sely = gry1;
                serx = grx2;
                sery = gry2;
                ldi = (sely - sery) / (selx - serx);
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
                    float disis;
                    float disiseps;
                    float disjeeps;
                    disis = dis(lx, ly, gxs[i], gys[i]);
                    disiseps = dis(lx, ly, gxs[i] * 0.999f + gxe[i] * 0.001f, gys[i] * 0.999f + gye[i] * 0.001f);
                    disjeeps = dis(lx, ly, gxe[j] * 0.999f + gxs[j] * 0.001f, gye[j] * 0.999f + gys[j] * 0.001f);
                    if (disiseps < disis)
                    {
                        cnt++;
                    }
                    if (disjeeps < disis)
                    {
                        cnt++;
                    }
                    if (cnt == 0)
                    {
                        if (cntorder(i, j, lx, ly, disis) == 0)
                        {
                            Debug.DrawLine(new Vector3((int)(lx), (int)(ly), 0), new Vector3((int)(lx) + 1, (int)(ly) + 1, 0));
                        }
                    }
                }//lx

                //bisector of i's right vertex and j's left vertex 
                bisec2poi(gxe[i], gye[i], gxs[j], gys[j]);
                eslx = grx1;
                esly = gry1;
                esrx = grx2;
                esry = gry2;
                //                Debug.DrawLine((int)(eslx),(int)(esly),(int)(esrx),(int)(esry));
                ldi = (esly - esry) / (eslx - esrx);
                lys = esly - ldi * eslx;
                //                for(lx=eslx;lx<esrx;lx=lx+0.5){
                z1start = eslx;
                z1end = esrx;
                if (Mathf.Abs(ldi) > 1.0f)
                {
                    if (ldi > 1.0f)
                    {
                        z1start = eslx * ldi + lys;
                        z1end = esrx * ldi + lys;
                    }
                    else
                    {
                        z1start = esrx * ldi + lys;
                        z1end = eslx * ldi + lys;
                    }
                }
                eslx = grx1;
                esly = gry1;
                esrx = grx2;
                esry = gry2;
                ldi = (esly - esry) / (eslx - esrx);
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
                    float disie;
                    float disieeps;
                    float disjseps;
                    disie = dis(lx, ly, gxe[i], gye[i]);
                    disieeps = dis(lx, ly, gxe[i] * 0.999f + gxs[i] * 0.001f, gye[i] * 0.999f + gys[i] * 0.001f);
                    disjseps = dis(lx, ly, gxs[j] * 0.999f + gxe[j] * 0.001f, gys[j] * 0.999f + gye[j] * 0.001f);
                    if (disieeps < disie)
                    {
                        cnt++;
                    }
                    if (disjseps < disie)
                    {
                        cnt++;
                    }
                    if (cnt == 0)
                    {
                        if (cntorder(i, j, lx, ly, disie) == 0)
                        {
                            Debug.DrawLine(new Vector3((int)(lx), (int)(ly), 0), new Vector3((int)(lx) + 1, (int)(ly) + 1, 0), Color.white);
                        }
                    }
                }//lx


                //bisector of two line segments  i,j
                bisec2seg(i, j);
                float lsegcsx1;
                float lsegcsy1;
                float lsegcex1;
                float lsegcey1;
                float lsegcsx2;
                float lsegcsy2;
                float lsegcex2;
                float lsegcey2;
                lsegcsx1 = gux1;
                lsegcsy1 = guy1;
                lsegcex1 = gux2;
                lsegcey1 = guy2;
                //                g.drawOval((int)(gx)-3,(int)(gy)-3,6,6);
                //                Debug.DrawLine((int)(lsegcsx1),(int)(lsegcsy1),(int)(lsegcex1),(int)(lsegcey1));
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
                lsegcsx1 = gux1;
                lsegcsy1 = guy1;
                lsegcex1 = gux2;
                lsegcey1 = guy2;
                ldi = (lsegcsy1 - lsegcey1) / (lsegcsx1 - lsegcex1);
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
                    intersectionxi = (ysi - gintercept[i]) / (gslope[i] - dii);//x coordinate of intersection of perpendicular line from (lx,ly) to line segment i and line segment i  線分iと(lx,ly)からiへ下ろした垂線の交点のx座標
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
                    intersectionxj = (ysj - gintercept[j]) / (gslope[j] - dij);//x coordinate of intersection of perpendicular line from (lx,ly) to line segment j and line segment j  線分jと(lx,ly)からjへ下ろした垂線の交点のx座標
                                                                               //if intersection is outside of line segment j then don't draw bisector
                    if (intersectionxj < gxs[j] || intersectionxj > gxe[j])
                    {
                        cnt++;
                    }
                    //cnt==0 means   perpendicular line from (lx,ly) to i crosses inside line segment i and perpendicular line from (lx,ly) to j crosses inside line segment j
                    if (cnt == 0)
                    {
                        float disijc;
                        disijc = Mathf.Abs(gslope[i] * lx - ly + gintercept[i]) / jou(jou(gslope[i], 2.0f) + 1.0f, 0.5f);//distance from (lx,ly) to segment i(same to sengemnt j)  (lx,ly)から線分iへの距離(jへの距離と同じです)
                                                                                                                     //compute distance to k. If distance to i is less than any other distance to k then plot (lx,ly) as bisector of i and j
                        if (cntorder(i, j, lx, ly, disijc) == 0)
                        {
                            Debug.DrawLine(new Vector3((int)(lx), (int)(ly), 0), new Vector3((int)(lx) + 1, (int)(ly) + 1, 0));
                        }
                    }
                }//lx

                //(gux3,guy3)-(gux4,guy4) is another bisector of an angle. Following 60 lines is same (gux1,guy1)-(gux2,guy2)
                lsegcsx2 = gux3;
                lsegcsy2 = guy3;
                lsegcex2 = gux4;
                lsegcey2 = guy4;
                //                Debug.DrawLine((int)(lsegcsx2),(int)(lsegcsy2),(int)(lsegcex2),(int)(lsegcey2));
                ldi = (lsegcsy2 - lsegcey2) / (lsegcsx2 - lsegcex2);
                lys = lsegcsy2 - ldi * lsegcsx2;
                //                for(lx=lsegcsx2;lx<lsegcex2;lx=lx+0.5){
                z1start = lsegcsx2;
                z1end = lsegcex2;
                if (Mathf.Abs(ldi) > 1.0f)
                {
                    if (ldi > 1.0f)
                    {
                        z1start = lsegcsx2 * ldi + lys;
                        z1end = lsegcex2 * ldi + lys;
                    }
                    else
                    {
                        z1start = lsegcex2 * ldi + lys;
                        z1end = lsegcsx2 * ldi + lys;
                    }
                }
                lsegcsx2 = gux3;
                lsegcsy2 = guy3;
                lsegcex2 = gux4;
                lsegcey2 = guy4;
                ldi = (lsegcsy2 - lsegcey2) / (lsegcsx2 - lsegcex2);
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
                    dii = -1.0f / gslope[i];
                    ysi = ly - dii * lx;
                    intersectionxi = (ysi - gintercept[i]) / (gslope[i] - dii);
                    if (intersectionxi < gxs[i] || intersectionxi > gxe[i])
                    {
                        cnt++;
                    }//intersectionxi<gxs[i] or intersectionxi>gxe[i]
                    float dij;
                    float ysj;
                    float intersectionxj;
                    dij = -1.0f / gslope[j];
                    ysj = ly - dij * lx;
                    intersectionxj = (ysj - gintercept[j]) / (gslope[j] - dij);
                    if (intersectionxj < gxs[j] || intersectionxj > gxe[j])
                    {
                        cnt++;
                    }
                    if (cnt == 0)
                    {
                        float disijc;
                        disijc = Mathf.Abs(gslope[i] * lx - ly + gintercept[i]) / jou(jou(gslope[i], 2.0f) + 1.0f, 0.5f);

                        if (cntorder(i, j, lx, ly, disijc) == 0)
                        {
                            Debug.DrawLine(new Vector3((int)(lx), (int)(ly), 0), new Vector3((int)(lx) + 1, (int)(ly) + 1, 0));
                        }
                    }
                }//lx


                //Logically, following 4 lines, next j next i  for(i=) for(j=) can be able to deleted. But loops are very long, so javac command has errors. So I added these 4 lines.
            }//j
        }//i

        for (i = 0; i < N - 1; i++)
        {
            for (j = i + 1; j < N; j++)
            {

                //bisector of segment and a point 
                //bisector is parabola arc.
                float lpboriginx;
                float lpboriginy;
                float x;
                float y;
                float xrmp;
                float yrmp;
                float xrmm;
                float yrmm;
                float ystart;
                float yend;

                float disjs;
                float disje;
                int cnt;

                float lpbijsp;
                float lpbijstheta;
                //parabola bisector of line segment i and left point of segment j
                bisecsegpois(i, j);
                lpbijsp = gpbijsp;
                lpbijstheta = gpi - gpbijstheta;
                if (gxs[j] > gpboriginx)
                {
                    lpbijstheta = gpi * 2.0f - gpbijstheta;
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
                for (y = ystart; y < yend; y = y + 0.1f)
                {
                    x = jou(y, 2.0f) / (4.0f * lpbijsp);
                    xrmp = lpboriginx + Mathf.Cos(-lpbijstheta) * x - Mathf.Sin(-lpbijstheta) * y;
                    yrmp = lpboriginy + Mathf.Sin(-lpbijstheta) * x + Mathf.Cos(-lpbijstheta) * y;
                    cnt = 0;
                    disjs = dis(xrmp, yrmp, gxs[j], gys[j]);
                    disje = dis(xrmp, yrmp, gxe[j], gye[j]);
                    if (disje < disjs)
                    {
                        cnt++;
                    }
                    else
                    {
                        float disjseps;
                        disjseps = dis(xrmp, yrmp, gxs[j] * 0.999f + gxe[j] * 0.001f, gys[j] * 0.999f + gye[j] * 0.001f);
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
                            Debug.DrawLine(new Vector3((int)(xrmp), (int)(yrmp), 0), new Vector3((int)(xrmp) + 1, (int)(yrmp) + 1, 0), Color.green);
                        }
                    }//cnt==0
                }//y

                float lpbijep;
                float lpbijetheta;
                //parabola bisector of line segment i and right point of segment j
                bisecsegpoie(i, j);
                lpbijep = gpbijep;
                lpbijetheta = gpi - gpbijetheta;
                if (gxe[j] > gpboriginx)
                {
                    lpbijetheta = gpi * 2.0f - gpbijetheta;
                }
                lpboriginx = gpboriginx;
                lpboriginy = gpboriginy;
                yisrm = Mathf.Sin(lpbijetheta) * (gxs[i] - lpboriginx) + Mathf.Cos(lpbijetheta) * (gys[i] - lpboriginy);
                yierm = Mathf.Sin(lpbijetheta) * (gxe[i] - lpboriginx) + Mathf.Cos(lpbijetheta) * (gye[i] - lpboriginy);
                ystart = yisrm;
                yend = yierm;
                if (yisrm > yierm)
                {
                    ystart = yierm;
                    yend = yisrm;
                }
                lpboriginx = gpboriginx;
                lpboriginy = gpboriginy;
                for (y = ystart; y < yend; y = y + 0.1f)
                {
                    x = jou(y, 2.0f) / (4.0f * lpbijep);
                    xrmp = lpboriginx + Mathf.Cos(-lpbijetheta) * x - Mathf.Sin(-lpbijetheta) * y;
                    yrmp = lpboriginy + Mathf.Sin(-lpbijetheta) * x + Mathf.Cos(-lpbijetheta) * y;
                    cnt = 0;
                    disjs = dis(xrmp, yrmp, gxe[j], gye[j]);
                    disje = dis(xrmp, yrmp, gxs[j], gys[j]);
                    if (disje < disjs)
                    {
                        cnt++;
                    }
                    else
                    {
                        float disjeeps;
                        disjeeps = dis(xrmp, yrmp, gxe[j] * 0.999f + gxs[j] * 0.001f, gye[j] * 0.999f + gys[j] * 0.001f);
                        if (disjeeps < disjs)
                        {
                            cnt++;
                        }
                    }
                    if (cnt == 0)
                    {
                        if (cntorder(i, j, xrmp, yrmp, disjs) == 0)
                        {
                            Debug.DrawLine(new Vector3((int)(xrmp), (int)(yrmp), 0), new Vector3((int)(xrmp) + 1, (int)(yrmp) + 1, 0), Color.magenta);
                        }
                    }//cnt==0
                }//y

                float lpbjisp;
                float lpbjistheta;
                //parabola bisector of line segment j and left point of segment i　
                bisecsegpois(j, i);
                lpbjisp = gpbijsp;
                lpbjistheta = gpi - gpbijstheta;
                if (gxs[i] > gpboriginx)
                {
                    lpbjistheta = gpi * 2.0f - gpbijstheta;
                }
                lpboriginx = gpboriginx;
                lpboriginy = gpboriginy;
                float yjsrm;
                float yjerm;
                yjsrm = Mathf.Sin(lpbjistheta) * (gxs[j] - lpboriginx) + Mathf.Cos(lpbjistheta) * (gys[j] - lpboriginy);
                yjerm = Mathf.Sin(lpbjistheta) * (gxe[j] - lpboriginx) + Mathf.Cos(lpbjistheta) * (gye[j] - lpboriginy);
                ystart = yjsrm;
                yend = yjerm;
                if (yjsrm > yjerm)
                {
                    ystart = yjerm;
                    yend = yjsrm;
                }
                lpboriginx = gpboriginx;
                lpboriginy = gpboriginy;
                for (y = ystart; y < yend; y = y + 0.1f)
                {
                    x = jou(y, 2.0f) / (4.0f * lpbjisp);
                    xrmp = lpboriginx + Mathf.Cos(-lpbjistheta) * x - Mathf.Sin(-lpbjistheta) * y;
                    yrmp = lpboriginy + Mathf.Sin(-lpbjistheta) * x + Mathf.Cos(-lpbjistheta) * y;
                    cnt = 0;
                    disjs = dis(xrmp, yrmp, gxs[i], gys[i]);
                    disje = dis(xrmp, yrmp, gxe[i], gye[i]);
                    if (disje < disjs)
                    {
                        cnt++;
                    }
                    else
                    {
                        float disiseps;
                        disiseps = dis(xrmp, yrmp, gxs[i] * 0.999f + gxe[i] * 0.001f, gys[i] * 0.999f + gye[i] * 0.001f);
                        if (disiseps < disjs)
                        {
                            cnt++;
                        }
                    }
                    if (cnt == 0)
                    {
                        if (cntorder(i, j, xrmp, yrmp, disjs) == 0)
                        {
                            Debug.DrawLine(new Vector3((int)(xrmp), (int)(yrmp), 0), new Vector3((int)(xrmp) + 1, (int)(yrmp) + 1, 0), Color.cyan);
                        }
                    }
                }//y

                float lpbjiep;
                float lpbjietheta;
                //parabola bisector of line segment j and right point of segment i　
                bisecsegpoie(j, i);
                lpbjiep = gpbijep;
                lpbjietheta = gpi - gpbijetheta;
                if (gxe[i] > gpboriginx)
                {
                    lpbjietheta = gpi * 2.0f - gpbijetheta;
                }
                lpboriginx = gpboriginx;
                lpboriginy = gpboriginy;
                yjsrm = Mathf.Sin(lpbjietheta) * (gxs[j] - lpboriginx) + Mathf.Cos(lpbjietheta) * (gys[j] - lpboriginy);
                yjerm = Mathf.Sin(lpbjietheta) * (gxe[j] - lpboriginx) + Mathf.Cos(lpbjietheta) * (gye[j] - lpboriginy);
                ystart = yjsrm;
                yend = yjerm;
                if (yjsrm > yjerm)
                {
                    ystart = yjerm;
                    yend = yjsrm;
                }
                lpboriginx = gpboriginx;
                lpboriginy = gpboriginy;
                for (y = ystart; y < yend; y = y + 0.1f)
                {
                    x = jou(y, 2.0f) / (4.0f * lpbjiep);
                    xrmp = lpboriginx + Mathf.Cos(-lpbjietheta) * x - Mathf.Sin(-lpbjietheta) * y;
                    yrmp = lpboriginy + Mathf.Sin(-lpbjietheta) * x + Mathf.Cos(-lpbjietheta) * y;
                    cnt = 0;
                    disjs = dis(xrmp, yrmp, gxe[i], gye[i]);
                    disje = dis(xrmp, yrmp, gxs[i], gys[i]);
                    if (disje < disjs)
                    {
                        cnt++;
                    }
                    else
                    {
                        float disieeps;
                        disieeps = dis(xrmp, yrmp, gxe[i] * 0.999f + gxs[i] * 0.001f, gye[i] * 0.999f + gys[i] * 0.001f);
                        if (disieeps < disjs)
                        {
                            cnt++;
                        }
                    }
                    if (cnt == 0)
                    {
                        if (cntorder(i, j, xrmp, yrmp, disjs) == 0)
                        {
                            Debug.DrawLine(new Vector3((int)(xrmp), (int)(yrmp), 0), new Vector3((int)(xrmp) + 1, (int)(yrmp) + 1, 0), Color.red);
                        }
                    }//cnt==0
                }//y

            }//j
        }//i
    }//paint main

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

    //parabola bisector of line segment i and left point of segment j
    void bisecsegpois(int pari, int parj)
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

        //distance between line i and point(left point of j)
        dis = Mathf.Abs(gslope[pari] * gxs[parj] - gys[parj] + gintercept[pari]) / jou(jou(gslope[pari], 2.0f) + 1.0f, 0.5f);
        lp = dis / 2.0f;
        lslopej = -1.0f / gslope[pari];
        linterceptj = gys[parj] - lslopej * gxs[parj];
        lintersectionx = (gintercept[pari] - linterceptj) / (lslopej - gslope[pari]);
        lintersectiony = lintersectionx * lslopej + linterceptj;
        lcx = (gxs[parj] + lintersectionx) / 2.0f;
        lcy = (gys[parj] + lintersectiony) / 2.0f;
        lthetaj = Mathf.Atan(lslopej);

        //(gpboriginx,gpboriginy) is origin point for segment i and edge point of j for y^2=4lpx.
        gpboriginx = lcx;
        gpboriginy = lcy;
        //gpbijsp is lp when bisector is y^2=4lpx for line segment i and edge point of j. 
        gpbijsp = lp;
        //gpbijstheta is angle of rotation when bisector is y^2=4lpx for line segment i and edge point of j. 
        gpbijstheta = lthetaj;

    }//bisecsegpois

    //parabola bisector of line segment pari and right point of segment parj　
    void bisecsegpoie(int pari, int parj)
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


        //distance between line pari and point(right point of parj)　
        dis = Mathf.Abs(gslope[pari] * gxe[parj] - gye[parj] + gintercept[pari]) / jou(jou(gslope[pari], 2.0f) + 1.0f, 0.5f);
        lp = dis / 2.0f;
        lslopej = -1.0f / gslope[pari];
        linterceptj = gye[parj] - lslopej * gxe[parj];
        lintersectionx = (gintercept[pari] - linterceptj) / (lslopej - gslope[pari]);
        lintersectiony = lintersectionx * lslopej + linterceptj;
        lcx = (gxe[parj] + lintersectionx) / 2.0f;
        lcy = (gye[parj] + lintersectiony) / 2.0f;
        lthetaj = Mathf.Atan(lslopej);

        //(gpboriginx,gpboriginy) is origin point for segment i and edge point of j for y^2=4lpx.
        gpboriginx = lcx;
        gpboriginy = lcy;
        //gpbijep is lp when bisector is y^2=4lpx for line segment i and edge point of j. 
        gpbijep = lp;
        //gpbijetheta is angle of rotation when bisector is y^2=4lpx for line segment i and edge point of j.
        gpbijetheta = lthetaj;

    }//bisecsegpoie

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
