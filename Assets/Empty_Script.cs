using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class Empty_Script : MonoBehaviour
{
    public class CAtom
    {
        public string type;
        public string name;
        public string ResName;
        public int ResID;
        public Vector3 position;
        public float BFactor;
        public List<CAtom> pBonds = new List<CAtom>();
        public GameObject Gobj;
        public void printPosition()
        {
            print("( " + position[0] + " , " + position[1] + " , " + position[2] + " )");
        }
        
    }
    List<CAtom> LAtoms = new List<CAtom>();
    List<List<CAtom>> AllHelix = new List<List<CAtom>>();
    List<List<CAtom>> StrandsGroup = new List<List<CAtom>>();
    List<List<List<CAtom>>> AllSheets = new List<List<List<CAtom>>>();
    public GameObject sphr;

    float CalcDistance(Vector3 v1,Vector3 v2)
    {
        float distance, xi, xj, yi, yj, zi, zj;
        xi = v1[0];
        yi = v1[1];
        zi = v1[2];
        xj = v2[0];
        yj = v2[1];
        zj = v2[2];
        distance = Mathf.Sqrt((xj - xi) * (xj - xi) + (yj - yi) * (yj - yi) + (zj - zi) * (zj - zi));
        return distance;    
    }

    void CreateBonds()
    {
        float distance;
        for (int i = 0; i < LAtoms.Count; i++)
        {
            for (int j = i + 1; j < LAtoms.Count; j++)
            {
                distance=CalcDistance(LAtoms[i].position, LAtoms[j].position);
                if (distance < 1.6)
                {
                    LAtoms[i].pBonds.Add(LAtoms[j]);
                    LAtoms[j].pBonds.Add(LAtoms[i]);
                }
            }
        }
    }
    int FindResd(string ResName,int ResID)
    {
        for(int i=0;i<LAtoms.Count;i++)
        {
            if (LAtoms[i].ResName == ResName && LAtoms[i].ResID==ResID)
                return i;
        }
        return -1;
    }
    void Helix(string ResNameStart,int ResIDStart,string ResNameEnd,int ResIDEnd)
    {
        int i=FindResd(ResNameStart,ResIDStart);
        List<CAtom> SingleHelix = new List<CAtom>();
        for(int j=i;j<LAtoms.Count;j++)
        {
            if (i == -1)
                break;
            if (LAtoms[j].ResID == ResIDEnd + 1)
                break;
            SingleHelix.Add(LAtoms[j]);
        }
        AllHelix.Add(SingleHelix);
    }
    void AllHelixAdd(string m_Path)
    {
        m_Path = Application.dataPath;
        StreamReader sr = new StreamReader(m_Path + "//" + "1AFB.pdb");
        string line = sr.ReadLine();
        while(line != null)
        {
            if(line.StartsWith("HELIX"))
            {
                string ResNameStart=line.Substring(15,5);
                int ResIDStart = int.Parse(line.Substring(22, 4));
                string ResNameEnd = line.Substring(27, 5);
                int ResIDEnd = int.Parse(line.Substring(34, 4));
                print(ResNameStart + " " + ResIDStart + " " + ResNameEnd + " " + ResIDEnd);
                Helix(ResNameStart,ResIDStart,ResNameEnd,ResIDEnd);
            }
            line = sr.ReadLine();
        }
    }
    void ColorHelix()
    {
        print("The number of helices is : " + AllHelix.Count);
        int count = 0;
        for (int i = 0; i < AllHelix.Count; i++)
        {
            for (int j = 0; j < AllHelix[i].Count; j++)
            {
                Renderer Rnd = AllHelix[i][j].Gobj.GetComponent<Renderer>();
                Rnd.material.SetColor("_Color", Color.red);
                count++;
            }
        }
        print("The number of helix amino acids is : "+count);
    }
    void StrandsGroupAdd(string ResNameStart, int ResIDStart, string ResNameEnd, int ResIDEnd)
    {
        int i = FindResd(ResNameStart, ResIDStart);
        List<CAtom> Strand = new List<CAtom>();
        for (int j = i; j < LAtoms.Count; j++)
        {
            if (i == -1)
                break;
            if (LAtoms[j].ResID == ResIDEnd + 1)
                break;
            Strand.Add(LAtoms[j]);
        }
        StrandsGroup.Add(Strand);
    }
    void AllSheetsAdd(string m_Path)
        {
            m_Path = Application.dataPath;
            StreamReader sr = new StreamReader(m_Path + "//" + "1AFB.pdb");
            string line = sr.ReadLine();
            string flag =" ", sheetID;
            bool firstSheet=true;
            while (line!=null)
            {
                if(line.StartsWith("SHEET"))
                {
                    sheetID = line.Substring(12,3);
                    if (firstSheet)
                    {
                    flag = sheetID;
                    firstSheet = false;
                    }
                    if (flag!=sheetID)
                    {
                    AllSheets.Add(StrandsGroup);
                    flag = sheetID;
                    StrandsGroup = new List<List<CAtom>>();
                    }
                    string ResNameStart = line.Substring(17, 5);
                    int ResIDStart = int.Parse(line.Substring(23, 4));
                    string ResNameEnd = line.Substring(28, 5);
                    int ResIDEnd = int.Parse(line.Substring(34,4));
                    print(ResNameStart + " " + ResIDStart + " " + ResNameEnd + " " + ResIDEnd);
                    StrandsGroupAdd(ResNameStart, ResIDStart, ResNameEnd, ResIDEnd);
                }
                line= sr.ReadLine();
            }
            if (StrandsGroup != null)
                 AllSheets.Add(StrandsGroup);

    }
    void ColorSheets()
    {
        int count = 0;
        print("Sheets " + AllSheets.Count);
        for (int i = 0; i < AllSheets.Count; i++)
        { 
            for (int j = 0; j < AllSheets[i].Count; j++)
            {
                print("Strands of sheet  "+(i+1)+ ' ' + AllSheets[i].Count);
                print("Atoms of strand   " +  (i+1) + "  Of Sheet  "+(j+1)+"  " + AllSheets[i][j].Count);
                for (int z=0; z < AllSheets[i][j].Count ; z++)
                {
                    Renderer Rnd = AllSheets[i][j][z].Gobj.GetComponent<Renderer>();
                    Rnd.material.SetColor("_Color", Color.blue);
                    count++;
                }
            }
        }
        print("The number of sheets amino acids is : " + count);
    }

    void RotateAroundX(List<CAtom>LAtms,float theta)
    {
        for(int i = 0; i < LAtms.Count; i++)
        {
            LAtms[i].position[1] = LAtms[i].position[1] * Mathf.Cos(theta) - LAtms[i].position[2] * Mathf.Sin(theta);
            LAtms[i].position[2] = LAtms[i].position[1] * Mathf.Sin(theta) + LAtms[i].position[2] * Mathf.Cos(theta);
        }
    }

    void RotateAroundY(List<CAtom> LAtoms,float theta)
    {
        for (int i = 0; i < LAtoms.Count; i++)
        {
            LAtoms[i].position[0] = LAtoms[i].position[2] * Mathf.Sin(theta) + LAtoms[i].position[0] * Mathf.Cos(theta);
            LAtoms[i].position[2] = LAtoms[i].position[2] * Mathf.Cos(theta) - LAtoms[i].position[0] * Mathf.Sin(theta);
        }
    }

    void RotateAroundZ(List<CAtom> LAtoms,float theta)
    {
        for (int i = 0; i < LAtoms.Count; i++)
        {
             LAtoms[i].position[0] = LAtoms[i].position[0] * Mathf.Cos(theta) - LAtoms[i].position[1] * Mathf.Sin(theta);
             LAtoms[i].position[1] = LAtoms[i].position[0] * Mathf.Sin(theta) + LAtoms[i].position[1] * Mathf.Cos(theta);
        }
    }
    void RotateAroundArbitrary(List<CAtom> LAtoms,Vector3 v1,Vector3 v2, float RotationAngle)
    {
        float nx, ny, nz;
        float dx = v2.x - v1.x;
        float dy = v2.y - v1.y;
        float dz = v2.z - v1.z;
        //prepare theta
        float theta = Mathf.Atan2(dy, dx);
        //prepare phi
        float phi = Mathf.Atan2(Mathf.Sqrt(dy * dy + dx * dx), dz);

        for(int i = 0; i < LAtoms.Count; i++)
        {
            Vector3 pos = LAtoms[i].position;

            //Translate the points to the v1 points
            pos.x -= v1.x;
            pos.y -= v1.y;
            pos.z -= v1.z;

            //Rotate around z axis with negative theta, i.e. reverse the rotation around z axis by theta, imagine we have matrices and we multiply the last operation first, number 5
            nx= pos.x* Mathf.Cos(-theta)-pos.y*Mathf.Sin(-theta);
            ny = pos.x * Mathf.Sin(-theta) + pos.y * Mathf.Cos(-theta);
            nz = pos.z;
            pos = new Vector3(nx, ny, nz);

            //Rotate around y axis with negative phi, i.e. reverse the rotation around y axis by phi, number 4
            nz = pos.z * Mathf.Cos(-phi) - pos.x * Mathf.Sin(-phi);
            nx = pos.z * Mathf.Sin(-phi) + pos.x * Mathf.Cos(-phi);
            ny = pos.y;
            pos = new Vector3(nx, ny, nz);

            //Rotate around z axis by the angle of ratation given, number 3
            nx = pos.x * Mathf.Cos(RotationAngle) - pos.y * Mathf.Sin(RotationAngle);
            ny = pos.x * Mathf.Sin(RotationAngle) + pos.y * Mathf.Cos(RotationAngle);
            nz = pos.z;
            pos = new Vector3(nx, ny, nz);

            //Rotate around y axis with phi, number 2
            nz = pos.z * Mathf.Cos(phi) - pos.x * Mathf.Sin(phi);
            nx = pos.z * Mathf.Sin(phi) + pos.x * Mathf.Cos(phi);
            ny = pos.y;
            pos = new Vector3(nx, ny, nz);

            //Rotate around z axis with theta ,number 1
            nx = pos.x * Mathf.Cos(theta) - pos.y * Mathf.Sin(theta);
            ny = pos.x * Mathf.Sin(theta) + pos.y * Mathf.Cos(theta);
            nz = pos.z;
            pos = new Vector3(nx, ny, nz);

            //Detranslate the points back to their original place
            pos.x += v1.x;
            pos.y += v1.y;
            pos.z += v1.z;

            LAtoms[i].position = pos;

        }
    }
        // Start is called before the first frame update
        void Start()
    {
        print("Hello I am the empty 3D object constructor"+"\n");
        Debug.Log(sphr.name);
        string m_Path = Application.dataPath;
        StreamReader sr = new StreamReader(m_Path + "//" + "1AFB.pdb");
        string line = sr.ReadLine();
        int atomCount = 0;
        while (line != null)
        {
            if (line.StartsWith("ATOM"))
            {
                atomCount++;
                CAtom pnn = new CAtom();
                string xs, ys, zs;
                float xf, yf, zf;
                xs = line.Substring(30, 8);
                ys = line.Substring(39, 8);
                zs= line.Substring(47, 8);
                xf = float.Parse(xs);
                yf = float.Parse(ys);
                zf = float.Parse(zs);
                Vector3 v1 = new Vector3(xf, yf, zf);
                pnn.position =v1;
                pnn.type = "ATOM";
                pnn.name = line.Substring(12,4);
                pnn.ResName = line.Substring(17,5);
                pnn.ResID = int.Parse(line.Substring(22,4));
                pnn.BFactor = float.Parse(line.Substring(60,6));
                pnn.printPosition();
                print(pnn.name + "  " + pnn.ResName + "  " + pnn.ResID + "  " + pnn.BFactor);
                LAtoms.Add(pnn);

            }
            line = sr.ReadLine();
        }
        print("The atoms of the protein are : " + atomCount + "\n");
        sr.Close();
        //RotateAroundX(LAtoms,60f);
        //RotateAroundY(LAtoms,260f);
        //RotateAroundZ(LAtoms,160f);
        //RotateAroundArbitrary(LAtoms,new Vector3(1, 1, 1), new Vector3(2, 2, 2), 200f);
        for (int i = 0; i < LAtoms.Count; i++)
        {
            LAtoms[i].Gobj = Instantiate(sphr, LAtoms[i].position, Quaternion.identity);
            sphr = GameObject.Find("Sphere");
            Renderer Rnd = LAtoms[i].Gobj.GetComponent<Renderer>();
            Rnd.material.SetColor("_Color", Color.blue);
        }
        CreateBonds();
        AllHelixAdd(m_Path);
        AllSheetsAdd(m_Path);
        ColorHelix();
        ColorSheets();
    }

    // Update is called once per frame
    void Update()
    {
        //We use Debug.DrawLine() here in Update() because the line is not an object ,it is something drawn not gameobject
        //This way you are drawing a line between consecutive spheres only; not efficient or realistic
        /*
        for(int i=0;i<LAtoms.Count-1;i++)
        {
            Debug.DrawLine(LAtoms[i].position, LAtoms[i + 1].position, Color.green);
        }*/

        //This way you are drawing a line between each and every sphere; not efficient or realistic, complexity N*N
        /*
        for (int i=0;i<LAtoms.Count;i++)
        {
            for(int j=i+1;j<LAtoms.Count;j++)
            {
                Debug.DrawLine(LAtoms[i].position, LAtoms[j].position, Color.green);
            }
        }*/
        //Let us draw the lists in the lists according to the threshold specified and bonds to be created
        for (int i = 0; i < LAtoms.Count; i++)
        {
            for (int j = 0; j < LAtoms[i].pBonds.Count; j++)
                Debug.DrawLine(LAtoms[i].position, LAtoms[i].pBonds[j].position, Color.green);
        }
    }
}
