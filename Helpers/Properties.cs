using System;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct Properties : IComponentData
{

    public float wing_area;
    public float wing_span;
    public float tail_area;
    public float cl_slope0;         // slope of Cl-alpha curve
    public float cl0;               // intercept of Cl-alpha curve
    public float cl_slope1;         // post-stall slope of Cl-alpha curve
    public float cl1;               // post-stall intercept of Cl-alpha curve
    public float alpha_cl_max;      // alpha when Cl=Clmax
    public float cdp;               // parasite drag coefficient
    public float eff;               // induced drag efficiency coefficient
    public float mass;
    public float engine_power;
    public float engine_rps;        // revolutions per second
    public float prop_diameter;
    public float a;                 //  propeller efficiency coefficient
    public float b;                 //  propeller efficiency coefficient

    public Properties(float wing_area, float wing_span, float tail_area, float cl_slope0, float cl0, float cl_slope1, float cl1, float alpha_cl_max, float cdp, float eff, float mass, float engine_power, float engine_rps, float prop_diameter, float a, float b)
    {
        this.wing_area = wing_area;
        this.wing_span = wing_span;
        this.tail_area = tail_area;
        this.cl_slope0 = cl_slope0;
        this.cl0 = cl0;
        this.cl_slope1 = cl_slope1;
        this.cl1 = cl1;
        this.alpha_cl_max = alpha_cl_max;
        this.cdp = cdp;
        this.eff = eff;
        this.mass = mass;
        this.engine_power = engine_power;
        this.engine_rps = engine_rps;
        this.prop_diameter = prop_diameter;
        this.a = a;
        this.b = b;
    }
    
}
