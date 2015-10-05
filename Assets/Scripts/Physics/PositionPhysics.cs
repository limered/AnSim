using UnityEngine;

namespace Assets.Scripts.Physics
{
    /// <summary>
    /// Calculates position/velocity information of a physics state.
    /// </summary>
    internal class PositionPhysics
    {
        private static Derivative evaluate(Vector3 force, Vector3 torque, State state)
        {
            Derivative res = new Derivative();
            res.velocity = state.velocity;
            res.spin = state.spin;
            res.force = force;
            res.torque = torque;
            return res;
        }

        private static Derivative evaluate(Vector3 force, Vector3 torque, State state, float dt, Derivative deriv)
        {
            state.position += deriv.velocity * dt;
            state.momentum += deriv.force * dt;

            for (var i = 0; i < 4; i++) state.orientation[i] += deriv.spin[i] * dt;
            state.angularMomentum += deriv.torque * dt;
            state.RecalculatePosition();
            state.RecalculateRotation();

            return evaluate(force, torque, state);
        }

        public static void IntegrateRK4(Vector3 force, Vector3 torque, State state, float dt)
        {
            //force /= 4f;
            //torque /= 4f;

            Derivative a = evaluate(force, torque, state);
            Derivative b = evaluate(force, torque, state, dt * 0.5f, a);
            Derivative c = evaluate(force, torque, state, dt * 0.5f, b);
            Derivative d = evaluate(force, torque, state, dt, c);

            state.position += 1.0f / 6.0f * dt * (a.velocity + 2.0f * (b.velocity + c.velocity) + d.velocity);
            state.momentum += 1.0f / 6.0f * dt * (a.force + 2.0f * (b.force + c.force) + d.force);

            state.orientation = AnSimMath.QuatAddQuat(state.orientation, AnSimMath.QuatScale(AnSimMath.QuatAddQuat(AnSimMath.QuatScale(AnSimMath.QuatAddQuat(b.spin, c.spin), 2f), AnSimMath.QuatAddQuat(a.spin, d.spin)), 1.0f / 6.0f * dt));

            state.angularMomentum += 1.0f / 6.0f * dt * (a.torque + 2.0f * (b.torque + c.torque) + d.torque);
            state.RecalculatePosition();
            state.RecalculateRotation();
        }
    }
}