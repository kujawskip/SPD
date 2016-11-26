namespace SPD.Engine
{
    public class SPDResult
    {
        public int[,] StrategyConfig { get; set; }
        public float[,] Points { get; set; }
        public bool Stabilization { get; set; }

        public SPDResult(float[,] points, int[,] strategyConfig, bool stabilization)
        {
            this.Points = points;
            this.StrategyConfig = strategyConfig;
            this.Stabilization = stabilization;
        }
    }
}