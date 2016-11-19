namespace SPD.Engine
{
    public class SPDResult
    {
        public int[,] strategyConfig;
        public float[,] v1;
        public bool v2;

        public SPDResult(float[,] v1, int[,] strategyConfig, bool v2)
        {
            this.v1 = v1;
            this.strategyConfig = strategyConfig;
            this.v2 = v2;
        }
    }
}