using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Condition
{
    A,
    B,
    C,
    D,
    E,
    F
}

public enum Relation
{
    And,
    Or,
    Not
}

public class TreeList
{
    public TreeList tRoot;
    public TreeList left;
    public TreeList right;

    public bool isRoot;
    public bool isRelation;
    public bool isCondition;
    public Relation rel;
    public Condition con;
    public int conValue;

    private Dictionary<TreeList, List<TreeList>> dic = new Dictionary<TreeList, List<TreeList>>();

    public TreeList()
    {

    }

    public void AddLeft(TreeList tl)
    {
        //左孩子
        left = tl;
        left.tRoot = this;
    }

    public void AddRight(TreeList tl)
    {
        //右兄弟
        right = tl;
        right.tRoot = this.tRoot;
    }

    public void MorrisIn()
    {
        TreeList root = this;
        if (root == null)
            return;
        TreeList cur1 = root;
        TreeList cur2 = null;
        while (cur1 != null)
        {
            cur2 = cur1.left;
            if (cur2 != null)
            {
                while (cur2.right != null && cur2.right != cur1)
                {
                    cur2 = cur2.right;
                }
                if (cur2.right == null)
                {
                    cur2.right = cur1;
                    cur1 = cur1.left;
                    continue;
                }
                else
                {
                    cur2.right = null;
                }
            }
            if(cur1.isCondition)
                Debug.Log(cur1.con + " ");
            else if(cur1.isRelation)
                Debug.Log(cur1.rel + " ");

            cur1 = cur1.right;
        }
        Debug.Log("\n");
    }

    public void MorrisPos()
    {
        TreeList root = this;
        if (root == null)
        {
            return;
        }
        TreeList cur1 = root;
        TreeList cur2 = null;
        while (cur1 != null)
        {
            cur2 = cur1.left;
            if (cur2 != null)
            {
                while (cur2.right != null && cur2.right != cur1)
                {
                    cur2 = cur2.right;
                }
                if (cur2.right == null)
                {
                    cur2.right = cur1;
                    cur1 = cur1.left;
                    continue;
                }
                else
                {
                    cur2.right = null;
                    PrintEdge(cur1.left);
                }
            }
            cur1 = cur1.right;
        }
        PrintEdge(root);
        Debug.Log("\n");
    }

    //void ReverseEdge(TreeList from);
    void PrintEdge(TreeList root)
    {
        TreeList tail = ReverseEdge(root);
        TreeList cur = tail;
        while (cur != null)
        {
            if (cur.isCondition)
            {//条件
                Debug.Log(cur.con + " ");
                if (dic.ContainsKey(cur.tRoot))
                {
                    var list = dic[cur.tRoot];
                    if (!list.Contains(cur))
                    {
                        list.Add(cur);
                    }
                }
                else
                {
                    List<TreeList> list = new List<TreeList>();
                    list.Add(cur);
                    dic.Add(cur.tRoot, list);
                }

            }
            else if (cur.isRelation)
            {//逻辑关系
                Debug.Log(cur.rel + " ");
                if (dic.ContainsKey(cur))
                {
                    Debug.Log("---------------- " + cur.rel + " ----------------");

                    foreach(var item in dic[cur])
                    {
                        Debug.Log("the value: " + item.con);
                    }

                    Debug.Log("---------------- " + cur.rel + " end ----------------");
                }
            }
            cur = cur.right;
        }
        ReverseEdge(tail);
    }

    TreeList ReverseEdge(TreeList from)
    {
        TreeList pre = null;
        TreeList next = null;
        while (from != null)
        {
            next = from.right;
            from.right = pre;
            pre = from;
            from = next;
        }
        return pre;
    }
}

public class ConditionTree : MonoBehaviour {
    public TreeList root;

	// Use this for initialization
	void Start () {
        //
        if(root == null)
            root = new TreeList();

        root.isRelation = true;
        root.isRoot = true;
        root.rel = Relation.And;

        var a = new TreeList();
        a.isCondition = true;
        a.isRoot = true;
        a.con = Condition.A;
        a.conValue = 1;
        root.AddLeft(a);

        var b = new TreeList();
        b.isCondition = true;
        b.isRoot = true;
        b.con = Condition.B;
        b.conValue = 0;
        a.AddRight(b);

        var or = new TreeList();
        or.isRelation = true;
        or.isRoot = true;
        or.rel = Relation.Or;
        b.AddRight(or);

        var c = new TreeList();
        c.isCondition = true;
        c.isRoot = true;
        c.con = Condition.C;
        c.conValue = 1;
        or.AddLeft(c);

        var and = new TreeList();
        and.isRelation = true;
        and.isRoot = true;
        and.rel = Relation.And;
        c.AddRight(and);

        var or1 = new TreeList();
        or1.isRelation = true;
        or1.isRoot = true;
        or1.rel = Relation.Or;
        and.AddLeft(or1);

        var d = new TreeList();
        d.isCondition = true;
        d.isRoot = true;
        d.con = Condition.D;
        d.conValue = 1;
        or1.AddRight(d);

        var e = new TreeList();
        e.isCondition = true;
        e.isRoot = true;
        e.con = Condition.E;
        e.conValue = 1;
        or1.AddLeft(e);

        var f = new TreeList();
        f.isCondition = true;
        f.isRoot = true;
        f.con = Condition.F;
        f.conValue = 1;
        e.AddRight(f);
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.A))
        {
            if(root != null)
            {
                //root.MorrisIn();
                root.MorrisPos();
            }
        }
		
	}
}
