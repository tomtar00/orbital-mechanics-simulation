namespace Sim
{
    public static class Time
    {
        public static float _timeScale = 1;
        public static float timeScale {
            get => _timeScale;
            set => _timeScale = value;
        }

        public static float deltaTime {
            get => UnityEngine.Time.deltaTime * timeScale;            
        } 
    }
}