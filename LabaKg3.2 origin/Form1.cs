using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;

namespace LabaKg3
{
    public partial class Form1 : Form
    {
        bool f = true;
        int k, l; // элементы матрицы сдвига
        float[,] kvSohran = new float[8, 4]; // Уменьшил размер, так как originalKv имеет 8 строк
        float[,] kv = new float[8, 4]; // матрица тела велосипеда (8 вершин)
        float[,] osi = new float[4, 3]; // матрица координат осей
        float[,] matr_sdv = new float[4, 4]; //матрица преобразования
        private double rotationAngle = 0;
        private double wheelRotationAngle = 0; // угол поворота колес
        private float[,] originalKv = new float[8, 4]; // Для хранения исходных координат (float)
        private const int wheelRadius = 48; // радиус колес
        private const int spokeCount = 8; // количество спиц в колесе
        public Form1()
        {
            InitializeComponent();
            Init_kvadrat(); // Инициализируем фигуру один раз при запуске
            Array.Copy(kv, originalKv, kv.Length); // Сохраняем исходное состояние
        }
        //обновление для осей
        private void DrawStaticAxes()
        {
            // Оси будут рисоваться относительно центра pictureBox
            int centerX = pictureBox1.Width / 2;
            int centerY = pictureBox1.Height / 2;

            Pen axisPen = new Pen(Color.Red, 1);
            Graphics g = Graphics.FromHwnd(pictureBox1.Handle);

            // Рисуем горизонтальную ось X
            g.DrawLine(axisPen, 0, centerY, pictureBox1.Width, centerY);

            // Рисуем вертикальную ось Y
            g.DrawLine(axisPen, centerX, 0, centerX, pictureBox1.Height);

            g.Dispose();
            axisPen.Dispose();
        }

        private void DrawFigureAndStaticAxes()
        {
            ClearDrawing(); // Очищаем поле
            /* DrawStaticAxes();*/ // Рисуем статичные оси поверх фигуры
        }
        private void ClearDrawing()
        {
            pictureBox1.Image = null;
            pictureBox1.Refresh();
        }
        private void ResetFigure()
        {
            Array.Copy(originalKv, kv, originalKv.Length);
            Draw_Kv(); // Перерисовываем фигуру в исходном положении
        }
        private void Init_kvadrat()
        {
            //Матрица имеет размер 8×4(8 вершин, по 4 координаты на каждую):
            //Первый индекс[i, ] -номер вершины(0-7)
            //Второй индекс[, j] -координаты:
            //j=0 - координата X
            //j = 1 - координата Y
            //j = 2 - координата Z
            //j = 3 - однородная координата(всегда 1)

            kv[0, 0] = 0; kv[0, 1] = 0; kv[0, 2] = 0; kv[0, 3] = 1;
            kv[1, 0] = 100; kv[1, 1] = 0; kv[1, 2] = 0; kv[1, 3] = 1;
            kv[2, 0] = 100; kv[2, 1] = 100; kv[2, 2] = 0; kv[2, 3] = 1;
            kv[3, 0] = 0; kv[3, 1] = 100; kv[3, 2] = 0; kv[3, 3] = 1;
            //верх
            kv[4, 0] = 0; kv[4, 1] = 100; kv[4, 2] = 100; kv[4, 3] = 1;
            kv[5, 0] = 100; kv[5, 1] = 100; kv[5, 2] = 100; kv[5, 3] = 1;
            kv[6, 0] = 100; kv[6, 1] = 0; kv[6, 2] = 100; kv[6, 3] = 1;
            kv[7, 0] = 0; kv[7, 1] = 0; kv[7, 2] = 100; kv[7, 3] = 1;

            // Сохраняем исходные координаты
            Array.Copy(kv, originalKv, kv.Length);
        }

        private void Draw_Kv()
        {
            Init_matr_preob(k, l); //инициализация матрицы сдвига
            float[,] kv1 = Multiply_matr(kv, matr_sdv); //перемножение матрицы фигуры на матрицу сдвига

            Pen myPen = new Pen(Color.Blue, 2);// цвет линии и ширина

            //создаем новый объект Graphics (поверхность рисования) из pictureBox
            Graphics g = Graphics.FromHwnd(pictureBox1.Handle);
            //нижняя грань
            g.DrawLine(myPen, kv1[0, 0], kv1[0, 1], kv1[1, 0], kv1[1, 1]);
            g.DrawLine(myPen, kv1[1, 0], kv1[1, 1], kv1[2, 0], kv1[2, 1]);
            g.DrawLine(myPen, kv1[2, 0], kv1[2, 1], kv1[3, 0], kv1[3, 1]);
            g.DrawLine(myPen, kv1[3, 0], kv1[3, 1], kv1[0, 0], kv1[0, 1]);
            //верхняя грань
            g.DrawLine(myPen, kv1[4, 0], kv1[4, 1], kv1[5, 0], kv1[5, 1]);
            g.DrawLine(myPen, kv1[5, 0], kv1[5, 1], kv1[6, 0], kv1[6, 1]);
            g.DrawLine(myPen, kv1[6, 0], kv1[6, 1], kv1[7, 0], kv1[7, 1]);
            g.DrawLine(myPen, kv1[7, 0], kv1[7, 1], kv1[4, 0], kv1[4, 1]);
            //ребра
            g.DrawLine(myPen, kv1[0, 0], kv1[0, 1], kv1[7, 0], kv1[7, 1]);
            g.DrawLine(myPen, kv1[1, 0], kv1[1, 1], kv1[6, 0], kv1[6, 1]);
            g.DrawLine(myPen, kv1[2, 0], kv1[2, 1], kv1[5, 0], kv1[5, 1]);
            g.DrawLine(myPen, kv1[3, 0], kv1[3, 1], kv1[4, 0], kv1[4, 1]);

            g.Dispose();// освобождаем все ресурсы, связанные с отрисовкой
            myPen.Dispose(); //освобождаем ресурсы, связанные с Pen
        }
        private void Init_matr_preob(int k1, int l1)
        {
            matr_sdv[0, 0] = 1; matr_sdv[0, 1] = 0; matr_sdv[0, 2] = 0; matr_sdv[0, 3] = 0;
            matr_sdv[1, 0] = 0; matr_sdv[1, 1] = 1; matr_sdv[1, 2] = 0; matr_sdv[1, 3] = 0;
            matr_sdv[2, 0] = 0; matr_sdv[2, 1] = 0; matr_sdv[2, 2] = 1; matr_sdv[2, 3] = 0;
            matr_sdv[3, 0] = k1; matr_sdv[3, 1] = l1; matr_sdv[3, 2] = 0; matr_sdv[3, 3] = 1;
        }

        private void Init_osi()
        {
            osi[0, 0] = -200; osi[0, 1] = 0; osi[0, 2] = 1;
            osi[1, 0] = 200; osi[1, 1] = 0; osi[1, 2] = 1;
            osi[2, 0] = 0; osi[2, 1] = 200; osi[2, 2] = 1;
            osi[3, 0] = 0; osi[3, 1] = -200; osi[3, 2] = 1;
        }
        // Метод для умножения матриц 4x4
        private double[,] MultiplyMatrices(double[,] a, double[,] b)
        {
            double[,] result = new double[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        result[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return result;
        }
        private float[,] Multiply_matr(float[,] a, float[,] b)
        {
            int n = a.GetLength(0);
            int m = b.GetLength(1);
            int m_a = a.GetLength(1);
            if (m_a != b.GetLength(0)) throw new Exception("Матрицы нельзя перемножить!");
            float[,] r = new float[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    r[i, j] = 0;
                    for (int ii = 0; ii < m_a; ii++)
                    {
                        r[i, j] += a[i, ii] * b[ii, j];
                    }
                }
            }
            return r;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //середина pictureBox
            k = pictureBox1.Width / 2;
            l = pictureBox1.Height / 2;
            Draw_Kv();
        }
        //private void Draw_osi()
        //{
        //    Init_osi();
        //    Init_matr_preob(k, l);
        //    int[,] osi1 = Multiply_matr(osi, matr_sdv);

        //    Pen myPen = new Pen(Color.Red, 1);
        //    Graphics g = Graphics.FromHwnd(pictureBox1.Handle);

        //    g.DrawLine(myPen, osi1[0, 0], osi1[0, 1], osi1[1, 0], osi1[1, 1]);
        //    g.DrawLine(myPen, osi1[2, 0], osi1[2, 1], osi1[3, 0], osi1[3, 1]);

        //    g.Dispose();
        //    myPen.Dispose();
        //}
        //Отрисовкка осей
        private void button1_Click(object sender, EventArgs e)
        {
            k = pictureBox1.Width / 2;
            l = pictureBox1.Height / 2;
            //Draw_osi();
        }
        //Таемер для сдвига
        private void button8_Click(object sender, EventArgs e)
        {
            //ClearDrawing();
            timer1.Interval = 100;

            button8.Text = "Стоп";
            if (f == true)
            {
                timer1.Start();

            }
            else
            {
                timer1.Stop();
                button8.Text = "Старт";
            }
            f = !f;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ClearDrawing();
            k -= 5; //изменение соответствующего элемента матрицы сдвига
            Draw_Kv(); //вывод квадратика
        }
        private void button4_Click(object sender, EventArgs e)
        {
            ClearDrawing();
            k += 5; //изменение соответствующего элемента матрицы сдвига
            Draw_Kv(); //вывод квадратика
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            k += 5; // Движение вправо
            wheelRotationAngle += 0.1; // Увеличиваем угол поворота

            ClearDrawing();
            DrawStaticAxes();
            Draw_Kv(); // Перерисовываем фигуру с новым сдвигом
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ClearDrawing();
            //DrawStaticAxes();
            l += 5; //изменение соответствующего элемента матрицы сдвига
            Draw_Kv(); //вывод квадратика
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ClearDrawing();
            DrawStaticAxes();
            l -= 5; //изменение соответствующего элемента матрицы сдвига
            Draw_Kv(); //вывод квадратика
        }
        //Масштабирование  фигуры на плоскости
        private void button9_Click(object sender, EventArgs e)
        {
            DrawFigureAndStaticAxes();
            if (textBox1.Text.Length == 0)
            {
                MessageBox.Show("Введите число!", "Внимание");
            }
            else
            {
                if (double.Parse(textBox1.Text.ToString()) < 0)
                {
                    MessageBox.Show("Число не может быть меньше 0", "");
                    textBox1.Clear();
                }
                else
                {
                    float n = float.Parse(textBox1.Text);
                    float[,] scaleMatrix = new float[4, 4] {
                        { n, 0, 0,0 },
                        { 0, n, 0,0 },
                        { 0, 0, n,0 },
                        { 0, 0, 0,1 }
                    };

                    // Применяем масштабирование к текущим координатам
                    float[,] temp = new float[8, 4];
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            temp[i, j] = 0;
                            for (int k = 0; k < 4; k++)
                            {
                                temp[i, j] += kv[i, k] * scaleMatrix[k, j];
                            }
                        }
                    }

                    // Обновляем текущие координаты фигуры
                    for (int i = 0; i < 8; i++)
                    {
                        kv[i, 0] = (float)temp[i, 0];
                        kv[i, 1] = (float)temp[i, 1];
                        kv[i, 2] = (float)temp[i, 2];
                        kv[i, 3] = (float)temp[i, 3];
                    }

                    // Отрисовка с текущими координатами
                    Draw_Kv();

                    textBox1.Clear();
                }
            }
        }

        //private void DrawFigureWithCurrentState()
        //{
        //    ClearDrawing();
        //    //DrawStaticAxes();

        //    // Применяем текущий сдвиг
        //    float[,] kv1 = new float[8, 4];
        //    for (int i = 0; i < 8; i++)
        //    {
        //        kv1[i, 0] = kv[i, 0] + k;
        //        kv1[i, 1] = kv[i, 1] + l;
        //        kv1[i, 2] = 1;
        //    }

        //    Pen myPen = new Pen(Color.Blue, 2);
        //    Graphics g = Graphics.FromHwnd(pictureBox1.Handle);
        //    g.DrawLine(myPen, kv1[0, 0], kv1[0, 1], kv1[1, 0], kv1[1, 1]);
        //    g.DrawLine(myPen, kv1[1, 0], kv1[1, 1], kv1[2, 0], kv1[2, 1]);
        //    g.DrawLine(myPen, kv1[2, 0], kv1[2, 1], kv1[3, 0], kv1[3, 1]);
        //    g.DrawLine(myPen, kv1[3, 0], kv1[3, 1], kv1[0, 0], kv1[0, 1]);
        //    //верхняя грань
        //    g.DrawLine(myPen, kv1[4, 0], kv1[4, 1], kv1[5, 0], kv1[5, 1]);
        //    g.DrawLine(myPen, kv1[5, 0], kv1[5, 1], kv1[6, 0], kv1[6, 1]);
        //    g.DrawLine(myPen, kv1[6, 0], kv1[6, 1], kv1[7, 0], kv1[7, 1]);
        //    g.DrawLine(myPen, kv1[7, 0], kv1[7, 1], kv1[4, 0], kv1[4, 1]);
        //    //ребра
        //    g.DrawLine(myPen, kv1[0, 0], kv1[0, 1], kv1[7, 0], kv1[7, 1]);
        //    g.DrawLine(myPen, kv1[1, 0], kv1[1, 1], kv1[6, 0], kv1[6, 1]);
        //    g.DrawLine(myPen, kv1[2, 0], kv1[2, 1], kv1[5, 0], kv1[5, 1]);
        //    g.DrawLine(myPen, kv1[3, 0], kv1[3, 1], kv1[4, 0], kv1[4, 1]);
        //    g.Dispose();
        //    myPen.Dispose();
        //}


        //Поворот фигуры
        private void button10_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(textBox2.Text, out double userAngle))
            {
                MessageBox.Show("Введите число");
                return;
            }

            DrawFigureAndStaticAxes();

            // Увеличиваем угол поворота на введенное значение (циклически)
            rotationAngle = (rotationAngle + userAngle) % 360;

            // Преобразуем угол в радианы для математических функций
            double angleInRadians = rotationAngle * Math.PI / 180;

            // Создаем матрицу поворота вокруг оси X (можно изменить на Y или Z при необходимости)
            double cosA = Math.Cos(angleInRadians);
            double sinA = Math.Sin(angleInRadians);

            double[,] rotationMatrixX = new double[4, 4]
            {
                { 1, 0, 0, 0 },
                { 0, cosA, sinA, 0 },
                { 0, -sinA, cosA, 0 },
                { 0, 0, 0, 1 }
            };

            // Временный массив для хранения повернутых координат (double)
            double[,] temp = new double[8, 4];

            // Применяем поворот к каждой вершине
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    temp[i, j] = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        temp[i, j] += kv[i, k] * rotationMatrixX[k, j];
                    }
                }
            }

            // Обновляем текущие координаты фигуры
            for (int i = 0; i < 8; i++)
            {
                kv[i, 0] = (float)temp[i, 0];
                kv[i, 1] = (float)temp[i, 1];
                kv[i, 2] = (float)temp[i, 2];
                kv[i, 3] = (float)temp[i, 3];
            }

            // Отрисовка повернутой фигуры с учетом текущего сдвига
            Draw_Kv();

            
        }


        //Отражение фигуры относительно Х
        private void button11_Click(object sender, EventArgs e)
        {
            DrawFigureAndStaticAxes();
            // Матрица отражения относительно оси X
            float[,] reflectX = new float[4, 4]
            {
                { 1, 0, 0, 0 },
                { 0, -1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            };

            // Применяем отражение к текущим координатам
            float[,] temp = Multiply_matr(kv, reflectX);

            // Обновляем текущие координаты фигуры
            for (int i = 0; i < 8; i++)
            {
                kv[i, 0] = temp[i, 0];
                kv[i, 1] = temp[i, 1];
                kv[i, 2] = temp[i, 2];
                kv[i, 3] = temp[i, 3];
            }

            // Рисуем отраженную фигуру с учетом текущего сдвига
            Draw_Kv();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            DrawFigureAndStaticAxes();
            // Матрица отражения относительно оси Y
            float[,] reflectY = new float[4, 4]
            {
                { -1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            };

            // Применяем отражение к текущим координатам
            float[,] temp = Multiply_matr(kv, reflectY);

            // Обновляем текущие координаты фигуры
            for (int i = 0; i < 8; i++)
            {
                kv[i, 0] = temp[i, 0];
                kv[i, 1] = temp[i, 1];
                kv[i, 2] = temp[i, 2];
                kv[i, 3] = temp[i, 3];
            }

            // Рисуем отраженную фигуру с учетом текущего сдвига
            Draw_Kv();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            DrawFigureAndStaticAxes();
            // Матрица отражения относительно начала координат
            float[,] reflectOrigin = new float[4, 4]
            {
                { -1, 0, 0, 0 },
                { 0, -1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            };

            // Применяем отражение к текущим координатам
            float[,] temp = Multiply_matr(kv, reflectOrigin);

            // Обновляем текущие координаты фигуры
            for (int i = 0; i < 8; i++)
            {
                kv[i, 0] = temp[i, 0];
                kv[i, 1] = temp[i, 1];
                kv[i, 2] = temp[i, 2];
                kv[i, 3] = temp[i, 3];
            }

            // Рисуем отраженную фигуру с учетом текущего сдвига
            Draw_Kv();
        }

        //Очистка PictureBox1
        private void button3_Click(object sender, EventArgs e)
        {
            ClearDrawing();
        }


        private void button14_Click(object sender, EventArgs e)
        {
            k = pictureBox1.Width / 2;
            l = pictureBox1.Height / 2;
            ResetFigure(); // Возвращаем фигуру в исходное положение и центр
        }
        //создание матрицы для поворота на угол (вокруг оси X)
        private double[,] CreateRotationMatrix(double angle)
        {
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);

            return new double[4, 4]
            {
                { 1, 0, 0, 0 },
                { 0, cosA, sinA, 0 },
                { 0, -sinA, cosA, 0 },
                { 0, 0, 0, 1 }
            };
        }

        private double[,] CreateRotationMatrix2(double angle) // Поворот вокруг оси Y
        {
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);

            return new double[4, 4]
            {
                { cosA, 0, -sinA, 0 },
                { 0, 1, 0, 0 },
                { sinA, 0, cosA, 0 },
                { 0, 0, 0, 1 }
            };
        }
    }
}