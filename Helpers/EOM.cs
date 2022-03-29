using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class EOM
{
    //-----------------------------------------------------
    // calculates the forces associated with an aircraft
    // given a set of properties and current state
    //-----------------------------------------------------
    public static void plane_rhs(Properties prop, State state, float[] q, float[] delta_q, float dt, float q_scale, ref float[] dq)
    {
        // Gravity
        float G = -9.81f;

        // property convenience variables
        float wing_area = prop.wing_area;
        float wing_span = prop.wing_span;
        float cl_slope0 = prop.cl_slope0;
        float cl0 = prop.cl0;
        float cl_slope1 = prop.cl_slope1;
        float cl1 = prop.cl1;
        float alpha_cl_max = prop.alpha_cl_max;
        float cdp = prop.cdp;
        float eff = prop.eff;
        float mass = prop.mass;
        float engine_power = prop.engine_power;
        float engine_rps = prop.engine_rps;
        float prop_diameter = prop.prop_diameter;
        float a = prop.a;
        float b = prop.b;

        // state convenience variables
        float alpha = state.alpha;
        float throttle = state.throttle;
        float flap = state.flap;

        // convert bank angle from degrees to radians
        // angle of attack is not converted because the
        // Cl-alpha curve is defined in terms of degrees
        float bank = state.bank * Mathf.Deg2Rad;

        // compute the intermediate values of the dependent variables
        float[] new_q = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        for (int i = 0; i < 6; i++) { new_q[i] = q[i] + q_scale * delta_q[i]; }

        // assign convenenience variables to the intermediate
        // values of the locations and velocities
        float vx = new_q[0];
        float vy = new_q[2];
        float vz = new_q[4];
        float x = new_q[1];
        float y = new_q[3];
        float z = new_q[5];
        float vh = Mathf.Sqrt(vx * vx + vy * vy);
        float vtotal = Mathf.Sqrt(vx * vx + vy * vy + vz * vz);

        // compute the air density
        float temperature = 288.15f - 0.0065f * z;
        float grp = 1.0f - 0.0065f * z / 288.15f;
        float pressure = 101325.0f * Mathf.Pow(grp, 5.25f);
        float density = 0.00348f * pressure / temperature;

        // compute power drop-off factor
        float omega = density / 1.225f;
        float factor = (omega - 0.12f) / 0.88f;

        // compute thrust 
        float advance_ratio = vtotal / (engine_rps * prop_diameter);
        float thrust = throttle * factor * engine_power * (a + b * advance_ratio * advance_ratio) / (engine_rps * prop_diameter);

        // compute lift coefficient - the Cl curve is modeled using two straight lines
        float cl = 0.0f;
        if (alpha < alpha_cl_max) { cl = cl_slope0 * alpha + cl0; }
        else { cl = cl_slope1 * alpha + cl1; }

        // include effects of flaps and ground effects
        // -- ground effects are present if the plane is within 5 meters of the ground
        if (flap == 20.0) { cl += 0.25f; }
        if (flap == 40.0) { cl += 0.5f; }
        if (z < 5.0) { cl += 0.25f; }

        // compute lift
        float lift = 0.5f * cl * density * vtotal * vtotal * wing_area;

        // compute drag coefficient
        float aspect_ratio = wing_span * wing_span / wing_area;
        float cd = cdp + cl * cl / (Mathf.PI * aspect_ratio * eff);

        // compute drag force
        float drag = 0.5f * cd * density * vtotal * vtotal * wing_area;

        // define some shorthand convenience variables for use with the rotation matrix
        // compute the sine and cosines of the climb angle, bank angle, and heading angle
        float cos_w = Mathf.Cos(bank);
        float sin_w = Mathf.Sin(bank);

        float cos_p;   //  climb angle
        float sin_p;   //  climb angle
        float cos_t;   //  heading angle
        float sin_t;   //  heading angle
        if (vtotal == 0.0) 
        {
            cos_p = 1.0f;
            sin_p = 0.0f;
        } else
        {
            cos_p = vh / vtotal;
            sin_p = vz / vtotal;
        }
        if (vh == 0.0)
        {
            cos_t = 1.0f;
            sin_t = 0.0f;
        }
        else
        {
            cos_t = vx / vh;
            sin_t = vy / vh;
        }

        // convert the thrust, drag, and lift forces into x-, y-, and z-components using the rotation matrix
        float fx = cos_t * cos_p * (thrust - drag) + (sin_t * sin_w - cos_t * sin_p * cos_w) * lift;
        float fy = sin_t * cos_p * (thrust - drag) + (-cos_t * sin_w - sin_t * sin_p * cos_w) * lift;
        float fz = sin_p * (thrust - drag) + cos_p * cos_w * lift;

        // add the gravity force to the z-direction force. 
        fz = fz + mass * G;

        // since the plane can't sink into the ground, if the altitude is less than or equal to zero and the z-component
        // of force is less than zero, set the z-force to be zero
        if (z <= 0.0 && fz <= 0.0) { fz = 0.0f; }

        // load the right-hand sides of the ODE's
        dq[0] = dt * (fx / mass);
        dq[1] = dt * vx;
        dq[2] = dt * (fy / mass);
        dq[3] = dt * vy;
        dq[4] = dt * (fz / mass);
        dq[5] = dt * vz;

    }

    //-----------------------------------------------------
    // solves the equations of motion using the Runge-Kutta
    // integration method
    //-----------------------------------------------------
    public static void eom(Properties prop, ref State state, float dt)
    {
        float[] q = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        float[] dq1 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        float[] dq2 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        float[] dq3 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        float[] dq4 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

        // retrieve the current values of the dependent and independent variables
        q[0] = state.q0;
        q[1] = state.q1;
        q[2] = state.q2;
        q[3] = state.q3;
        q[4] = state.q4;
        q[5] = state.q5;

        // compute the four Runge-Kutta steps, then return 
        // value of planeRightHandSide method is an array
        // of delta-q values for each of the four steps
        plane_rhs(prop, state, q, q, dt, 0.0f, ref dq1);
        plane_rhs(prop, state, q, dq1, dt, 0.5f, ref dq2);
        plane_rhs(prop, state, q, dq2, dt, 0.5f, ref dq3);
        plane_rhs(prop, state, q, dq3, dt, 1.0f, ref dq4);

        // update simulation time
        state.time += dt;

        // update the dependent and independent variable values
        // at the new dependent variable location and store the
        // values in the ODE object arrays
        for ( int i = 0; i < 6; i++ ) { q[i] = q[i] + (dq1[i] + 2.0f * dq2[i] + 2.0f * dq3[i] + dq4[i]) / 6.0f; }
        state.q0 = q[0];
        state.q1 = q[1];
        state.q2 = q[2];
        state.q3 = q[3];
        state.q4 = q[4];
        state.q5 = q[5];

    }

}
