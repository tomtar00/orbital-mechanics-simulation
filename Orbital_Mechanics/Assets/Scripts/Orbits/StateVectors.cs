using Sim.Math;

namespace Sim.Orbits {
    public class StateVectors
    {
        public Vector3Double position;
        public Vector3Double velocity;

        public StateVectors() {
            position = velocity = Vector3Double.zero;
        }

        public StateVectors(Vector3Double pos, Vector3Double vel)
        {
            this.position = pos;
            this.velocity = vel;
        }

        public StateVectors(StateVectors vectors) {
            this.position = vectors.position;
            this.velocity = vectors.velocity;
        }

        public override string ToString()
        {
            return "position = " + position + " velocity = " + velocity; 
        }
    }
}