using System;

namespace Trigger.Battle
{

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
    
    public class TreeNode
    {
        public static int[] valuee = {5, 8, 6, 15, 19, 3, 1};

        TreeNode leftNode;
        TreeNode rightNode;
        public virtual bool Execute()
        {
            Type ty = this.GetType();

            return false;
        }
    }

    public class LogicNode : TreeNode
    {
        //logic relation
        Relation rel;
        public override bool Execute()
        {
            return base.Execute();
        }
    }

    public class ConditionNode : TreeNode
    {
        //condition
        //condition value
        public Condition con;
        public int conVal;
        public int equals;
        private bool result;

        public override bool Execute()
        {
            if(TreeNode.valuee[(int)con] > conVal && equals > 0)
            {
                result = true;
            }
            else if(TreeNode.valuee[(int)con] == conVal && equals == 0)
            {
                result = true;
            }
            else if(TreeNode.valuee[(int)con] < conVal && equals < 0)
            {
                result = true;
            }
            return result;
        }
    }

}