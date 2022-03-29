using System;
using Unity.Entities;

[System.Serializable]
public struct State : IComponentData
{

    public float time;          // time
    public float q0, q1, q2, q3, q4, q5;    // ODE results

    public float bank;          // roll angle
    public float alpha;         // pitch angle
    public float throttle;      // throttle percentage
    public float flap;          // flap deflection

    public State(float time, float q0, float q1, float q2, float q3, float q4, float q5, float bank, float alpha, float throttle, float flap)
    {
        this.time = time;
        this.q0 = q0;
        this.q1 = q1;
        this.q2 = q2;
        this.q3 = q3;
        this.q4 = q4;
        this.q5 = q5;
        this.bank = bank;
        this.alpha = alpha;
        this.throttle = throttle;
        this.flap = flap;
    }
    
}
