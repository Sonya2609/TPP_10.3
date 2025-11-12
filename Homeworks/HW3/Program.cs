using System;

class Polynomial
{
    private int degree;
    private double[] coeffs;

    public Polynomial()
    {
        degree = 0;
        coeffs = new double[1] { 0.0 };
    }

    public Polynomial(double[] new_coeffs)
    {
        degree = new_coeffs.Length - 1;
        coeffs = (double[])new_coeffs.Clone();
    }

    public int Degree
    {
        get { return degree; }
    }

    public double[] Coeffs
    {
        get { return (double[])coeffs.Clone(); }
    }

    public override string ToString()
    {
        if (this.coeffs.Length == 0) { return "0"; }
        string res = "";
        for (int i = 0; i < this.coeffs.Length; i++)
        {
            if (this.coeffs[i] == 0) { continue; }
            if (i == 0) { res += this.coeffs[i]; }
            else if (i == 1)
            {
                res += this.coeffs[i] < 0 ? "-" : "+";
                res += this.coeffs[i] == 1 ? "x" : Math.Abs(this.coeffs[i]).ToString() + "x";
            }
            else
            {
                res += this.coeffs[i] < 0 ? "-" : "+";
                res += Math.Abs(this.coeffs[i]).ToString() + "x^" + i;
            }
        }
        return res;
    }
    
    public static Polynomial operator + (Polynomial obj1, Polynomial obj2)
    {
        double[] resCoeffs = new double[Math.Max(obj1.Degree, obj2.Degree) + 1];
        for (int i = 0; i < resCoeffs.Length; i++)
        {
            double coeff1 = obj1.degree>=i? obj1.Coeffs[i]: 0.0;
            double coeff2 = obj2.degree>=i? obj2.Coeffs[i]: 0.0;
            resCoeffs[i] = coeff1 + coeff2;
        }
        return new Polynomial(resCoeffs);
    }
    
    public static Polynomial operator * (Polynomial obj1, double k)
    {
        double[] resCoeffs = new double[obj1.Degree + 1];
        for (int i = 0; i < resCoeffs.Length; i++)
        {
            resCoeffs[i] = obj1.Coeffs[i] * k;
        }
        return new Polynomial(resCoeffs);
    }
}

class Programm
{
    static void Main(string[] args)
    {
        double[] coeffs1 = { 1.0, 0.0, 2.0 };
        Polynomial p = new Polynomial(coeffs1); // 1 + 2x^2
        Console.WriteLine(p);

        double[] coeffs2 = { 2.0, 1.0, 0.0, -4.0 };
        Polynomial n = new Polynomial(coeffs2); // 2 + x - 4x^3
        Console.WriteLine(n);

        Polynomial sum = p + n;
        Console.WriteLine(sum); // 3 + x + 2x^2 - 4x^3

        Polynomial multipl = sum*4.0;
        Console.WriteLine(multipl); // 12 + 4x + 8x^2 - 16x^3
    }
}