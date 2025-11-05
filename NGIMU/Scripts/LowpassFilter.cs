using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ButterworthFilter
{
    private double[] _a;
    private double[] _b;
    private double[] _x;
    private double[] _y;

    public ButterworthFilter()
    {}

    public ButterworthFilter(double[] b, double[] a)
    {
        // Normalize coefficients
        double a0 = a[0];
        _b = b.Select(x => x / a0).ToArray();
        _a = a.Select(x => x / a0).ToArray();

        // Initialize state variables
        int order = _a.Length;// - 1;
        _x = new double[order];
        _x = _x.Select(x => 0.0).ToArray();
        _y = new double[order];
        _y = _y.Select(x => 0.0).ToArray();
    }

    public double Filter(double input)
    {
        // Shift state variables
        for (int i = _x.Length - 1; i > 0; i--)
        {
            _x[i] = _x[i - 1];
            _y[i] = _y[i - 1];
        }

        // Update input
        _x[0] = input;

        // Compute output
        double output = _b[0]*_x[0];
        for (int i = 1; i < _b.Length; i++)
        {
            output += _b[i] * _x[i];
            output -= _a[i] * _y[i];
        }

        // Update output
        _y[0] = output;

        return output;
    }
}


