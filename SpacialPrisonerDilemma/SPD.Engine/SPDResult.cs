using SPD.Engine.Strategies;

namespace SPD.Engine
{
    public class SPDResult
    {
        private int[,] strategyConfig;
        private float[,] v1;
        private bool v2;

        public SPDResult(float[,] v1, int[,] strategyConfig, bool v2)
        {
            this.v1 = v1;
            this.strategyConfig = strategyConfig;
            this.v2 = v2;
        }
    }
}