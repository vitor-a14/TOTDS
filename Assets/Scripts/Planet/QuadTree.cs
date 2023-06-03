using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeNode 
{
    //0 top left
    //1 top right
    //2 bottom right
    //3 bottom left
    public Vector3 center;
    public QuadTreeNode parent;
    public QuadTreeNode root;
    
    public QuadTreeNode[] children;
    public float radius;
    public int detailLevel;

    public Vector3 localUp;
    public Vector3 axisA;
    public Vector3 axisB;
    public Vector3[] vertices;
    public Vector3[] normals;
    public int[] triangles;
    public Vector3[] verticesBorder;
    public Vector3[] normalsBorder;
    public int[] trianglesBorder;
    public float planetRadius;
    public byte corner;
    public uint hash = 0;
    public bool[] neighbours;
    public LOD lod;
    Transform planetTransform;

    public bool[] edgeNeighbours;
    public int[] edgeDirections;

    public QuadTreeNode(Vector3 center, QuadTreeNode root, QuadTreeNode parent, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, byte corner, uint hash,Transform planetTransform,float planetRadius,LOD lod) {
        this.center = center;
        this.parent = parent;
        this.root = root;
        this.radius = radius;
        this.detailLevel = detailLevel;
        this.localUp = localUp;
        this.axisA = axisA;
        this.axisB = axisB;
        this.children = new QuadTreeNode[0];
        this.corner = corner;
        this.hash = hash;
        this.planetTransform = planetTransform;
        this.planetRadius = planetRadius;
        neighbours = new bool[4] { false, false, false, false };
        edgeNeighbours = new bool[4] { false, false, false, false };
        edgeDirections = new int[2] { -1, -1 };
        this.lod = lod;
    }

    public void CreateChildren() {
        if (detailLevel >= lod.MaxDetail) {
            return;
        }

        if (Vector3.Distance(planetTransform.TransformPoint(center.normalized * planetRadius) , LOD.cameraPos.position) > lod.detailLevelDist[detailLevel]) {
            return;
        }

        this.children = new QuadTreeNode[4];
        this.children[0] = new QuadTreeNode(center + (axisA * radius) / 2 - (axisB * radius) / 2, root, this, radius / 2, detailLevel + 1, this.localUp, this.axisA, this.axisB, 0, hash * 4,this.planetTransform,planetRadius,lod);
        this.children[1] = new QuadTreeNode(center + (axisA * radius) / 2 + (axisB * radius) / 2, root, this, radius / 2, detailLevel + 1, this.localUp, this.axisA, this.axisB, 1, hash * 4 + 1,this.planetTransform, planetRadius,lod);
        this.children[2] = new QuadTreeNode(center - (axisA * radius) / 2 + (axisB * radius) / 2, root, this, radius / 2, detailLevel + 1, this.localUp, this.axisA, this.axisB, 2, hash * 4 + 2,this.planetTransform, planetRadius,lod);
        this.children[3] = new QuadTreeNode(center - (axisA * radius) / 2 - (axisB * radius) / 2, root, this, radius / 2, detailLevel + 1, this.localUp, this.axisA, this.axisB, 3, hash * 4 + 3,this.planetTransform, planetRadius,lod);

        for (int i = 0; i < 4; i++) {
            this.children[i].CreateChildren();
        }
    }

    public void UpdateChildren() {
        if (detailLevel >= lod.MaxDetail) {
            return;
        }

        //local to world space conversion
        if (Vector3.Distance(planetTransform.TransformPoint(center.normalized * planetRadius), LOD.cameraPos.position) > lod.detailLevelDist[detailLevel]) {
            children = new QuadTreeNode[0];
            return;
        } else {
            if (children.Length > 0) {
                for (int i = 0; i < 4; i++) {
                    children[i].UpdateChildren();
                }
            } else {
                vertices = null;
                triangles = null;
                normals = null;
                CreateChildren();
            }
        }
    }

    //if a neighbor is bigger than node that neighbor is true
    //directions specified in direction class
    public void FindNeighbors() {
        for (int i = 0; i < 4; i++) {
            neighbours[i] = false;
        }
        QuadTreeNode n1;
        QuadTreeNode n2;
        int dir1;
        int dir2;

        //top left
        if (corner == 0) {
            dir1 = DIRECTION.NORTH;
            dir2 = DIRECTION.WEST;
        }
        else if (corner == 1) {
            dir1 = DIRECTION.NORTH;
            dir2 = DIRECTION.EAST;
        }
        else if (corner == 2) {
            dir1 = DIRECTION.EAST;
            dir2 = DIRECTION.SOUTH;
        }
        else {
            dir1 = DIRECTION.WEST;
            dir2 = DIRECTION.SOUTH;
        }

        n1 = FindNeighborsBiggerEqual(dir1);
        n2 = FindNeighborsBiggerEqual(dir2);
        n1 = FindLeafNeighbour(n1, dir1);
        n2 = FindLeafNeighbour(n2, dir2);
        if (n1 != null) {
            if (n1.children.Length == 0 && n1.detailLevel < this.detailLevel) {
                neighbours[dir1] = true;
            }
        }
        
        if (n2 != null) {
            if (n2.children.Length == 0 && n2.detailLevel < this.detailLevel) {
                neighbours[dir2] = true;
            }
        }
    }

    //this finds common parent and get child node of that parent in given direction
    public QuadTreeNode FindNeighborsBiggerEqual(int direction) {
        if (parent == null) {
            return null;
        }

        if (corner == 0) {
            if (direction == DIRECTION.SOUTH) {
                return parent.children[3];
            } else if (direction == DIRECTION.EAST) {
                return parent.children[1];
            }
        } else if (corner == 1) {
            if (direction == DIRECTION.SOUTH) {
                return parent.children[2];
            } else if (direction == DIRECTION.WEST) {
                return parent.children[0];
            }
        } else if (corner == 2) {
            if (direction == DIRECTION.WEST) {
                return parent.children[3];
            } else if (direction == DIRECTION.NORTH) {
                return parent.children[1];
            }
        } else if (corner == 3) {
            if (direction == DIRECTION.EAST) {
                return parent.children[2];
            } else if (direction == DIRECTION.NORTH) {
                return parent.children[0];
            }
        }

        return parent.FindNeighborsBiggerEqual(direction);
    }

    //1-aa-bb  format encode path 
    //convert hash value to array 1-01-01-10-11 -> 1123  left most bit is start bit given in root node
    public int[] DecryptHash() {
        int[] loc = new int[9] { -1, -1, -1, -1, -1, -1, -1, -1 ,-1};
        uint tmphash = hash;
        int index = detailLevel - 1;

        while (tmphash > 1) {
            int num = (int)(tmphash & 3); //get last two bit this gives position on path
            loc[index] = num;
            index -= 1;
            tmphash = tmphash >> 2; 
        }

        return loc;
    }

    //when neighbor parent is found search tree to find neighbor node 
    //follow path
    public QuadTreeNode FindLeafNeighbour(QuadTreeNode parent, int direction) {
        if (parent == null) {
            return null;
        }

        bool axisX = false;
        if (direction == DIRECTION.WEST || direction == DIRECTION.EAST) {
            axisX = true;
        }

        int[] path = DecryptHash();
        int index = parent.detailLevel;             
        QuadTreeNode t = parent;

        while (t.children.Length != 0 && path[index] != -1) {
            if (axisX == false) {
                t = t.children[DIRECTION.MIRROR_AXIS_X[path[index]]]; //neighbor node mirrors path so use mirrored  direction  horizontal mirror
            } else {
                t = t.children[DIRECTION.MIRROR_AXIS_Y[path[index]]]; //vertical mirror
            }
            index += 1;
        }

        if (t.children.Length == 0) {
            return t;
        }

        return null;
    }

    //find nodes that has at least one edge at edge of cube face
    public bool QuadTreeEdgeFind() {
        int[] array = DecryptHash();
        bool isEdge1 = true;
        bool isEdge2 = true;
        edgeDirections[0] = -1;
        edgeDirections[1] = -1;
        
        //0 1
        //3 2
        
        //0 north  west     1 north east     3 south west
        //1 north  east     0 north west     2 south east
        //2 south  east     1 north east     3 south west
        //3 south  west     0 north west     2 south east

        //for 0
        //   ^ ^
        // <-0 1  only west and north directions leads to an edge so 1 can lead noth edge or 3 can lead to west edge
        // <-3 2  
        int otherPossibleCorner1;
        int otherPossibleCorner2;
        int non; //imposible corner

        if (corner == 0) {
            otherPossibleCorner1 = 1;
            otherPossibleCorner2 = 3;
            non = 2;

        } else if (corner == 1) {
            otherPossibleCorner1 = 0;
            otherPossibleCorner2 = 2;
            non = 3;
        } else if (corner == 2) {
            otherPossibleCorner1 = 1;
            otherPossibleCorner2 = 3;
            non = 0;
        } else {
            otherPossibleCorner1 = 0;
            otherPossibleCorner2 = 2;
            non = 1;
        }

        int i = 0;
        bool search = true;

        //change this if more depth than 8 
        while (i < 8 && search == true) {
            if (array[i] == -1) {
                search = false;
            } else if (array[i] == non) {
                isEdge1 = false;
                isEdge2 = false;
                search = false;
            } else if (array[i] == otherPossibleCorner1) {
                isEdge2 = false;
            } else if (array[i] == otherPossibleCorner2) {
                isEdge1 = false;
            }
            i += 1;
        }

        if (isEdge1) {
            if (corner == 0) {
                edgeDirections[0] = DIRECTION.NORTH;
            } else if (corner == 1) {
                edgeDirections[0] = DIRECTION.NORTH;
            } else if (corner == 2) {
                edgeDirections[0] = DIRECTION.EAST;
            } else {
                edgeDirections[0] = DIRECTION.WEST;
            }
        }

        if (isEdge2) {
            if (corner == 0) {
                edgeDirections[1] = DIRECTION.WEST;
            } else if (corner == 1) {
                edgeDirections[1] = DIRECTION.EAST;
            } else if (corner == 2) {
                edgeDirections[1] = DIRECTION.SOUTH;
            } else {
                edgeDirections[1] = DIRECTION.SOUTH;
            }
        }

        return isEdge1 | isEdge2;
    }
}

//tree structure used for lod mesh
public class Quadtree {
    public LOD lod;
    public int maxDetailLevel;
    public float radius;
    public Vector3 center;
    public Vector3 localUp;
    public Vector3 AxisA;
    public Vector3 AxisB;
    
    public Vector3[] normals;
    public float planetRadius;
    public int res;
    public QuadTreeNode[] leafNodes;

    public int verticeCount;
    public int triangleCount;

    public int verticeCountCollision;
    public int triangleCountCollision;

    public QuadTreeNode root;

    //nodes converted for parallel programming
    public QuadTreeNodeJob[] leafnodeJobs;
    public QuadTreeNodeJob[] leafnodeJobsCollision;

    public int leafNodeCount;
    public int collisionLeafNodeCount;
    public Transform planetCenter;

    public Plane plane;
    public int collisionDetailLevel;

    public Quadtree(int maxDetailLevel, float radius, Vector3 center, Vector3 localUp, Vector3 AxisA, Vector3 AxisB, int res, PlanetMesh t,Plane p) {
        this.maxDetailLevel = maxDetailLevel;
        this.radius = radius;
        this.center = center;
        this.localUp = localUp;
        this.AxisA = AxisA;
        this.AxisB = AxisB;
        this.res = res;
        planetCenter = t.transform;
        this.plane = p;
        this.planetRadius = p.planetRadius;
        this.lod = p.Lod;
        this.collisionDetailLevel = p.collisionDetailLevel;
        leafNodeCount = 0;
        collisionLeafNodeCount = 0;

        //preallocate max size 
        this.leafNodes = new QuadTreeNode[100000];
        this.leafnodeJobs = new QuadTreeNodeJob[100000];
        this.leafnodeJobsCollision = new QuadTreeNodeJob[100000];
    }

    //get connecting edge detailLevel  used for removing seams
    public int GetNeighbourQuadTreeDetailLevel(QuadTreeNode node, int direction) {
        Plane neighborPlane = plane.neighbours[direction];
        int connectDirection = plane.neighborConnectDirection[direction];
        int[] nodePath = node.DecryptHash();
        int len = nodePath.Length;
        for (int i = 0; i < node.detailLevel; i++) {
            nodePath[i] = DIRECTION.DIRECTION_MAP[direction, connectDirection, nodePath[i]];
        }

        //search through neighbor quadtree;
        Quadtree neighborTree = neighborPlane.quadtree;
        QuadTreeNode searchNode = neighborTree.root;
        int index = 0;
        while (searchNode.children.Length != 0 && index < node.detailLevel) {
            searchNode = searchNode.children[nodePath[index]];
            index += 1;

        }

        return searchNode.detailLevel;
    }

    //toggle edge neighbor bool if true remove seam in the job task
    public void UpdateEdgeNeighbors() {
        int len = leafNodeCount;
        int depth1;
        int depth2; 

        for (int i = 0; i < len; i++) {
            leafNodes[i].edgeNeighbours[0] = false;
            leafNodes[i].edgeNeighbours[1] = false;
            leafNodes[i].edgeNeighbours[2] = false;
            leafNodes[i].edgeNeighbours[3] = false;
            if (leafNodes[i].edgeDirections[0] != -1) {
                depth1 = GetNeighbourQuadTreeDetailLevel(leafNodes[i], leafNodes[i].edgeDirections[0]);
                if (depth1 < leafNodes[i].detailLevel) {
                    leafNodes[i].edgeNeighbours[leafNodes[i].edgeDirections[0]] = true;
                }
            }

            if (leafNodes[i].edgeDirections[1] != -1) {
                depth2 = GetNeighbourQuadTreeDetailLevel(leafNodes[i], leafNodes[i].edgeDirections[1]);
                if (depth2 < leafNodes[i].detailLevel) {
                    leafNodes[i].edgeNeighbours[leafNodes[i].edgeDirections[1]] = true;
                }
            }
        }
    }

    public void GenerateTree() {
        root = new QuadTreeNode(center, root, null, radius, 0, localUp, AxisA, AxisB, 0, 1,planetCenter,planetRadius,lod);
        root.CreateChildren();
    }

    public void FindNeighbors() {
        int len = leafNodeCount;
        for (int i = 0; i < len; i++) {
            leafNodes[i].FindNeighbors();
        }    
    }

    public void ConvertJobs() {
        int len = leafNodeCount;
        for(int i = 0; i < len; i++) {
            leafnodeJobs[i].center = leafNodes[i].center;
            leafnodeJobs[i].axisA = leafNodes[i].axisA;
            leafnodeJobs[i].axisB = leafNodes[i].axisB;
            leafnodeJobs[i].detailLevel = leafNodes[i].detailLevel;
            leafnodeJobs[i].localUp = leafNodes[i].localUp;
            leafnodeJobs[i].radius = leafNodes[i].radius;
        
            leafnodeJobs[i].neighbours0 = leafNodes[i].neighbours[0];
            leafnodeJobs[i].neighbours1= leafNodes[i].neighbours[1];
            leafnodeJobs[i].neighbours2= leafNodes[i].neighbours[2];
            leafnodeJobs[i].neighbours3 = leafNodes[i].neighbours[3];

            leafnodeJobs[i].edgeDirection1 = leafNodes[i].edgeDirections[0];
            leafnodeJobs[i].edgeDirection2 = leafNodes[i].edgeDirections[1];

            leafnodeJobs[i].edgeNeighbors0 = leafNodes[i].edgeNeighbours[0];
            leafnodeJobs[i].edgeNeighbors1 = leafNodes[i].edgeNeighbours[1];
            leafnodeJobs[i].edgeNeighbors2 = leafNodes[i].edgeNeighbours[2];
            leafnodeJobs[i].edgeNeighbors3 = leafNodes[i].edgeNeighbours[3];

            leafnodeJobs[i].verticeIndexStart = i * (res * res);
            leafnodeJobs[i].triangleIndexStart = i * ((res -1)* (res - 1) *6);
        }

        verticeCount = len * (res * res);
        triangleCount= len * ((res - 1) * (res - 1) * 6);
        GetCollisionLeafTree();
    }

    public void ConvertJobsV2() {
        int len = leafNodeCount;
        for (int i = 0; i < len; i++) {
            leafNodes[i].FindNeighbors();
            leafnodeJobs[i].center = leafNodes[i].center;
            leafnodeJobs[i].axisA = leafNodes[i].axisA;
            leafnodeJobs[i].axisB = leafNodes[i].axisB;
            leafnodeJobs[i].detailLevel = leafNodes[i].detailLevel;
            leafnodeJobs[i].localUp = leafNodes[i].localUp;
            leafnodeJobs[i].radius = leafNodes[i].radius;

            leafnodeJobs[i].neighbours0 = leafNodes[i].neighbours[0];
            leafnodeJobs[i].neighbours1 = leafNodes[i].neighbours[1];
            leafnodeJobs[i].neighbours2 = leafNodes[i].neighbours[2];
            leafnodeJobs[i].neighbours3 = leafNodes[i].neighbours[3];

            leafnodeJobs[i].edgeDirection1 = leafNodes[i].edgeDirections[0];
            leafnodeJobs[i].edgeDirection2 = leafNodes[i].edgeDirections[1];

            leafnodeJobs[i].edgeNeighbors0 = leafNodes[i].edgeNeighbours[0];
            leafnodeJobs[i].edgeNeighbors1 = leafNodes[i].edgeNeighbours[1];
            leafnodeJobs[i].edgeNeighbors2 = leafNodes[i].edgeNeighbours[2];
            leafnodeJobs[i].edgeNeighbors3 = leafNodes[i].edgeNeighbours[3];


            leafnodeJobs[i].verticeIndexStart = i * (res * res);
            leafnodeJobs[i].triangleIndexStart = i * ((res - 1) * (res - 1) * 6);
        }

        verticeCount = len * (res * res);
        triangleCount = len * ((res - 1) * (res - 1) * 6);
        GetCollisionLeafTree();
    }
    
    public void UpdateTree() {
        root.UpdateChildren();
    }

    //get leaf nodes for collision mesh 
    public void GetCollisionLeafTree() {
        collisionLeafNodeCount = 0;
        int len = leafNodeCount;
        int c = 0;
        for(int i = 0; i< len; i++) {
            if (leafNodes[i].detailLevel >= collisionDetailLevel) {
                leafnodeJobsCollision[c].center = leafNodes[i].center;
                leafnodeJobsCollision[c].axisA = leafNodes[i].axisA;
                leafnodeJobsCollision[c].axisB = leafNodes[i].axisB;
                leafnodeJobsCollision[c].detailLevel = leafNodes[i].detailLevel;
                leafnodeJobsCollision[c].localUp = leafNodes[i].localUp;
                leafnodeJobsCollision[c].radius = leafNodes[i].radius;

                leafnodeJobsCollision[c].neighbours0 = leafNodes[i].neighbours[0];
                leafnodeJobsCollision[c].neighbours1 = leafNodes[i].neighbours[1];
                leafnodeJobsCollision[c].neighbours2 = leafNodes[i].neighbours[2];
                leafnodeJobsCollision[c].neighbours3 = leafNodes[i].neighbours[3];

                leafnodeJobsCollision[c].verticeIndexStart = c * (res * res);
                leafnodeJobsCollision[c].triangleIndexStart = c * ((res - 1) * (res - 1) * 6);
                
                c += 1;
            }

            verticeCountCollision = c * (res * res);
            triangleCountCollision = c * ((res - 1) * (res - 1) * 6);
        }

        collisionLeafNodeCount = c;
        return;
    }

    public void setupMeshDataPartition() {
        GetLeafNodes();
        ConvertJobs();
    }

    public void GetLeafNodes() {
        leafNodeCount = 0;
        DFS(root);
    }

    public void DFS(QuadTreeNode node) {
        if (node.children.Length > 0) {
            for (int i = 0; i < node.children.Length; i++) {
                DFS(node.children[i]);
            }
        } else {
            node.QuadTreeEdgeFind();
            leafNodes[leafNodeCount] = node;
            leafNodeCount += 1;
        }
    }
}
