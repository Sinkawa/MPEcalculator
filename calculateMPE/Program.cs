using System;
using System.Security.Policy;

namespace calculateMPE
{
    class Data
    {
        public double height;
        public double length;
        public double width;
        public double averageSpeed;
        public double concentration;
        public int deltaTemperature;
        public double mpc;
        public double diameter;
        public double m;
        public double n;
        public double Vm;
        public double Vi;

        public bool isDust;
        
        public Data()
        {
            height = length = width = diameter = averageSpeed = concentration = mpc = -1.0;
            deltaTemperature = -1;
            m = n = Vm = Vi = -1.0;
            isDust = false;
        }
    }
    
    internal class Program
    {

        private static Data readFromConsole()
        {
            Data data = new Data();
            Console.WriteLine("Является ли вещество пылью? (0/1): ");
            int b = Convert.ToInt32(Console.ReadLine());
            if (b == 0)
                data.isDust = false;
            else
                data.isDust = true;
            Console.WriteLine("Введите высоту источника сброса, м: ");
            data.height = Convert.ToDouble(Console.ReadLine());
            Console.WriteLine("Устье прямоугольной формы? (0/1): ");
            b = Convert.ToInt32(Console.ReadLine());
            if (b == 1)
            {
                Console.WriteLine("Введите длину устья, м: ");
                data.length = Convert.ToDouble(Console.ReadLine());
               
                data.width = Convert.ToDouble(Console.ReadLine());
            }
            else
            {
                Console.WriteLine("Введите диаметр устья, м: ");
                data.diameter = Convert.ToDouble(Console.ReadLine());
            }

            Console.WriteLine("Введите среднюю скорость выхода ГВС, м/с: ");
            data.averageSpeed = Convert.ToDouble(Console.ReadLine());
            Console.WriteLine("Введите фактическую концентрацию, мг/м3: ");
            data.concentration = Convert.ToDouble(Console.ReadLine());
            Console.WriteLine("Введите разность температур ГВС и АВ, 'C: ");
            data.deltaTemperature = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Введите ПДК вещества, мг/м3: ");
            data.mpc = Convert.ToDouble(Console.ReadLine());
            return data;
        }

        private static String ComparisonResult(double MPE, double M)
        {
            if (M > MPE)
            {
                return "Фактический выброс превышает предельно-допустимый.";
            }

            if (M == MPE)
            {
                return "Фактический выброс равен предельно-допустимому.";
            }

            return "Фактический выброс не превышает предельно-допустимый.";
        }
        
        public static void Main(string[] args)
        {
            Data data = readFromConsole();
            double MPE = GetMPE(data);
            Console.WriteLine($"Предельно-допустимый выброс: {MPE}.");
            double M = data.concentration * data.Vi * 0.001;
            Console.WriteLine($"Фактический выброс: {M}.");
            Console.WriteLine(ComparisonResult(MPE, M));
        }

        private static double GetMPE(Data initData)
        {
            var pD = initData;
            double bC, result;
            double F = 1;

            if (pD.isDust)
            {
                F = 2.5;
            }
            
            bC = pD.mpc * 0.3;
            pD.diameter = getDiameter(pD);
            pD.Vi = (Math.PI * Math.Pow(pD.diameter, 2.0) * pD.averageSpeed) / 4;

            pD.Vm = getVm(pD);
            pD.n = getn(pD.Vm);
            pD.m = getm(getF(pD));
            
            if (initData.deltaTemperature == 0)
            {
                result = (pD.mpc - bC) * Math.Pow(pD.height, 4.0 / 3.0) * 8 * pD.Vi;
                result = result / (180 * F * pD.n * 1 * pD.diameter);
            }
            else
            {
                double x = pD.Vi * pD.deltaTemperature;
                result = (pD.mpc - bC) * Math.Pow(pD.height, 2.0) * Math.Pow(x, 1.0 / 3.0);
                result = result / (180 * F * pD.m * pD.n * 1);
            }

            return result;
        }

        private static double getn(double Vm)
        {
            if (Vm >= 2)
            {
                return 1;
            }
            else if (Vm < 0.5)
            {
                return 4.4 * Vm;
            }
            else return 0.532 * Math.Pow(Vm, 2.0) - 2.13 * Vm + 3.13;
        }
        
        private static double getm(double f)
        {
            if (f < 100.0)
            {
                return 1 / (0.67 + 0.1 * Math.Pow(f, 1.0 / 2.0) + 0.34 * Math.Pow(f, 1.0 / 3.0));
            }

            return 1.47 / Math.Pow(f, 1.0 / 3.0);
        }
        
        private static double getVm(Data data)
        {
            double x;
            if (data.deltaTemperature == 0)
            {
                x = 1.3 * data.averageSpeed * data.diameter / data.height;
                Console.WriteLine($"Коэффициент Vm: {x}.");
                return x;
            }

            x = data.Vi * data.deltaTemperature / data.height;
            x = 0.65 * Math.Pow(x, 1.0 / 3.0);
            Console.WriteLine($"Коэффициент Vm: {x}.");
            return x;
        }
        
        private static double getF(Data data)
        {
            double x;
            if (data.deltaTemperature == 0.0)
            {
                x = 800 * Math.Pow(data.Vm, 3.0);
                Console.WriteLine($"Коэффициент F: {x}.");
                return x;
            }
            
            x = 1000.0 * Math.Pow(data.averageSpeed, 2.0) * data.diameter / 
                (Math.Pow(data.height, 2.0) * data.deltaTemperature);
            Console.WriteLine($"Коэффициент F: {x}.");
            return x;
        }
        
        private static double getDiameter(Data data)
        {
            if (data.diameter != -1)
            {
                return data.diameter;
            }
            
            return (2.0 * data.length * data.width) /
                   (data.length + data.width);
        }
    }
}