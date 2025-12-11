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

    public static Polynomial operator +(Polynomial obj1, Polynomial obj2)
    {
        double[] resCoeffs = new double[Math.Max(obj1.Degree, obj2.Degree) + 1];
        for (int i = 0; i < resCoeffs.Length; i++)
        {
            double coeff1 = obj1.degree >= i ? obj1.Coeffs[i] : 0.0;
            double coeff2 = obj2.degree >= i ? obj2.Coeffs[i] : 0.0;
            resCoeffs[i] = coeff1 + coeff2;
        }
        return new Polynomial(resCoeffs);
    }

    public static Polynomial operator *(Polynomial obj, double k)
    {
        double[] resCoeffs = new double[obj.Degree + 1];
        for (int i = 0; i < resCoeffs.Length; i++)
        {
            resCoeffs[i] = obj.Coeffs[i] * k;
        }
        return new Polynomial(resCoeffs);
    }
    public static Polynomial operator *(double k, Polynomial obj)
    {
        return obj*k;
    }
    public static Polynomial operator *(Polynomial obj1, Polynomial obj2)
    {
        double[] resCoeffs = new double[obj1.Degree + obj2.Degree+1];
        for (int i = 0; i < obj1.coeffs.Length; i++)
        {
            for (int j = 0; j < obj2.coeffs.Length; j++)
            {
                resCoeffs[i+j] = obj1.Coeffs[i] * obj2.Coeffs[j];
            }
        }
        return new Polynomial(resCoeffs);
    }
    public double Evaluate(double x)
    {
        double result = 0.0;
        for(int i= 0;i< this.coeffs.Length; i++)
        {
            result += this.coeffs[i]*Math.Pow(x,i);
        }
        return result;
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

        Polynomial multipl = 4.0 * sum;
        Console.WriteLine(multipl); // 12 + 4x + 8x^2 - 16x^3

        Console.WriteLine(p*n); // 3 + x + - 4x^2 - 2x^3 - 8x^5

        Console.WriteLine(multipl.Evaluate(2.0)); // -76
    }
}