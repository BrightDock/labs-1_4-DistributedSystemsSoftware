using labs_1_4_DistributedSystemsSoftware.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;
using labs_1_4_DistributedSystemsSoftware.Hubs;

namespace labs_1_4_DistributedSystemsSoftware.Controllers
{
    public class oedController : Controller
    {
        private List<dataType> dataDB = new List<dataType>();
        private Thread PDThread;
        public void dataUpload()
        {
            var file = @Request.Files["file"];
            var userID = @Request.Form.Get("userID");
            var lab = @Request.Form.Get("lab");
            string filePath;
            if (!file.Equals(null))
            {
                string fileName;
                string json = string.Empty;
                fileName = file.FileName;
                Directory.CreateDirectory(Server.MapPath("~/Files/"));
                filePath = Path.Combine(Server.MapPath("~/Files/" + fileName));
                file.SaveAs(filePath);
                PDThread = new Thread(() => (this.GetType().GetMethod(lab)).Invoke(this, new object[] { filePath, userID }));

                if (!String.IsNullOrEmpty(filePath))
                {
                    json = Json("true", JsonRequestBehavior.AllowGet).Data.ToString();

                    PDThread.Start();
                }
                else {
                    json = Json("false", JsonRequestBehavior.AllowGet).Data.ToString();
                }
                Response.Write(json);
                Response.Flush();
                Response.Close();

            }
            else
            {
                throw new DirectoryNotFoundException();
            }
        }

        private List<double> parceData(string filePath, string userID, string column)
        {
            using (var stream = System.IO.File.OpenRead(filePath))
            using (var reader = new StreamReader(stream))
            {
                var parsed = parser.Parse(reader, ' ', '\n');
                double progress = 0.0;
                List<List<string>> data = parsed.Cast<List<string>>().ToList();
                var dataCount = data.Count();
                double fVal = 0.0;
                double sVal = 0.0;
                double tVal = 0.0;
                long prevPercent = 0;

                foreach (List<string> line in data)
                {
                    double.TryParse(line.ElementAt(0).ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US"), out fVal);
                    double.TryParse(line.ElementAt(1).ToString(), NumberStyles.Float, CultureInfo.GetCultureInfo("en-US"), out sVal);
                    double.TryParse(line.ElementAt(2).ToString(), NumberStyles.Float, CultureInfo.GetCultureInfo("en-US"), out tVal);

                    dataDB.Add(new dataType(fVal, sVal, tVal));
                    progress += 1.0;
                    if ((((Convert.ToInt64(progress) * 40 / dataCount) % 1 == 0) && (Convert.ToInt64(progress) * 40 / dataCount) % 10 != prevPercent) ||
                            (progress / dataCount == 1))
                    {
                        mainRHub.processProgress((progress * 40 / dataCount), userID);
                        //                        Thread.Sleep(50);
                    }
                    prevPercent = (Convert.ToInt64(progress) * 40 / dataCount) % 10;
                }
                progress = 0.0;

                var elemList = new List<double>();
                dataDB.ForEach(x => elemList.Add(
                    Convert.ToDouble(x.GetType().GetProperty(column).GetValue(x).ToString())
                ));

                return elemList;
            }
        }

        public ActionResult oedCS0(string filePath = "", string userID = "")
        {
            if (string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(@Request.Cookies.Get("file").Value))
            {
                var fileName = @Request.Params.Get("file");

                filePath = Path.Combine(Server.MapPath("~/Files/" + fileName));
                userID = @Request.Params.Get("userID");
            }

            if ((!string.IsNullOrEmpty(filePath) && !filePath.Equals("")))
            {
                mainRHub.NotifyByID("<h3>Обработка</h3><div class='notRow'>Обработка данных начата</div>", userID);

                using (var stream = System.IO.File.OpenRead(filePath))
                using (var reader = new StreamReader(stream))
                {
                    var parsed = parser.Parse(reader, ' ', '\n');
                    List<List<string>> data = parsed.Cast<List<string>>().ToList();
                    var dataCount = data.Count();
                    mainRHub.processProgress(0, userID);
                    var elemList = new List<double>();
                    var json = new Dictionary<string, string>();

                    elemList = parceData(filePath, userID, "secondVal");

                    double sMean = 0.0;
                    sMean = sampleMean<double>(elemList);
                    mainRHub.processProgress(46.6, userID);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Среднее значение");
                    json.Add("value", sMean.ToString("#0.00000"));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);
                    //                Thread.Sleep(50);

                    double sQuadratic = 0.0;
                    sQuadratic = selectiveQuadratic<double>(elemList);
                    mainRHub.processProgress(53.3, userID);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Выборочная дисперсия");
                    json.Add("value", sQuadratic.ToString("#0.00000"));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);
                    //                Thread.Sleep(50);

                    double pointEstimateOfSQ = 0.0;
                    pointEstimateOfSQ = pointEstimateOfSelectiveQuadratic(sQuadratic);
                    Hubs.mainRHub.processProgress(59.9, userID);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Среднеквадратическое отклонение");
                    json.Add("value", pointEstimateOfSQ.ToString("#0.00000"));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);
                    //                Thread.Sleep(50);

                    double EOfSQ = 0.0;
                    EOfSQ = EstimateOfSelectiveQuadratic(sQuadratic, elemList.Count);
                    mainRHub.processProgress(65.5, userID);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Оценка среднеквадратического отклонения результатов измерения");
                    json.Add("value", EOfSQ.ToString("#0.00000"));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

                    int iCount = 0;
                    iCount = intervalsCount(dataCount);
                    mainRHub.processProgress(71.9, userID);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Количество интервалов");
                    json.Add("value", iCount.ToString());
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

                    double sSize = 0;
                    sSize = stepSize(dataDB, "secondVal", iCount);
                    mainRHub.processProgress(78.5, userID);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Шаг равен");
                    json.Add("value", sSize.ToString("#0.00000"));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

                    json = new Dictionary<string, string>();
                    mainRHub.processProgress(84.1, userID);
                    json.Add("message", "min и max значения");
                    json.Add("value", minMaxValue(dataDB, "secondVal").ToString());
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

                    List<double> intervals = new List<double>();
                    intervals = getIntervals(minMaxValue(dataDB, "secondVal").Item1, iCount, sSize);
                    mainRHub.processProgress(90.7, userID);
                    var intervalsInString = getIntervalsToString(intervals);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Интервалы");
                    json.Add("value", string.Join("<br />", intervalsInString));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

                    sendDataOfCharts(intervals, "Диаграмма интервалов", "Интервал", userID);

                    List<int> chartIntervalsElementsCount = new List<int>();
                    chartIntervalsElementsCount = getChartIntervalsElementsCount(intervals, elemList);
                    mainRHub.processProgress(97.4, userID);

                    sendDataOfCharts(chartIntervalsElementsCount, "Количетво элементов в интервале", "Интервал", userID, intervalsInString);

                    Tuple<double, double> hisquraredOfV = new Tuple<double, double>(0, 0);
                    hisquraredOfV = hiSquared(intervals, chartIntervalsElementsCount, sMean, sQuadratic, userID, iCount);
                    mainRHub.processProgress(100, userID);
                    json = new Dictionary<string, string>();
                    json.Add("message", "χ² наблюдаемое и χ² критическое");
                    json.Add("value", string.Join("<br />", hisquraredOfV));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

                    json = new Dictionary<string, string>();
                    json.Add("title", "Выводы");
                    mainRHub.sendConclusionOfCalculating(JsonConvert.SerializeObject(json), userID, mainRHub.typeOfSendingString.rowsTitle);

                    json = new Dictionary<string, string>();
                    if (hisquraredOfV.Item1 > hisquraredOfV.Item2)
                    {
                        json.Add("conclusion", "Так как χ² наблюдаемое больше χ² критического, то нулевую гипотезу следует отвергнуть, распределение генеральной совокупности не соответствует нормальному закону.");
                    }
                    else
                    {
                        json.Add("conclusion", "Так как χ² наблюдаемое меньше или равна χ² критического, то нулевую гипотезу следует принять, распределение генеральной совокупности соответствует нормальному закону.");
                    }
                    mainRHub.sendConclusionOfCalculating(JsonConvert.SerializeObject(json), userID);

                    mainRHub.NotifyByID(String.Format("<h3>Обработка</h3><div class='notRow'>Обработка окончена</div><div class='notRow'>Обработано: {0} записей</div>", dataDB.Count), userID);
                }
            }

            return PartialView();
        }

        public ActionResult oedCS1(string filePath = "", string userID = "")
        {
            if (string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(@Request.Cookies.Get("file").Value))
            {
                var fileName = @Request.Params.Get("file");

                filePath = Path.Combine(Server.MapPath("~/Files/" + fileName));
                userID = @Request.Params.Get("userID");
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                mainRHub.NotifyByID("<h3>Обработка</h3><div class='notRow'>Обработка данных начата</div>", userID);

                using (var stream = System.IO.File.OpenRead(filePath))
                using (var reader = new StreamReader(stream))
                {
                    var parsed = parser.Parse(reader, ' ', '\n');
                    List<List<string>> data = parsed.Cast<List<string>>().ToList();
                    var dataCount = data.Count();
                    Hubs.mainRHub.processProgress(0, userID);
                    var elemList = new List<double>();
                    var json = new Dictionary<string, string>();

                    elemList = parceData(filePath, userID, "thirdVal");

                    int iCount = 0;
                    double sSize = 0;
                    iCount = intervalsCount(dataCount);
                    sSize = stepSize(dataDB, "thirdVal", iCount);

                    List<double> intervals = new List<double>();
                    intervals = getIntervals(minMaxValue(dataDB, "thirdVal").Item1, iCount, sSize);
                    Hubs.mainRHub.processProgress(50, userID);
                    var intervalsInString = getIntervalsToString(intervals);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Интервалы");
                    json.Add("value", string.Join("<br />", intervalsInString));
                    Hubs.mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

                    double expectedV = 0.0;
                    expectedV = expectedValue(elemList);
                    Hubs.mainRHub.processProgress(60, userID);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Оценка математического ожидания");
                    json.Add("value", expectedV.ToString("#0.00000"));
                    Hubs.mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

                    sendDataOfCharts(intervals, "Диаграмма интервалов", "Интервал", userID);
                }
            }
            return PartialView();
        }

        public ActionResult oedCS2(string filePath = "", string userID = "")
        {
            if (string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(@Request.Cookies.Get("file").Value))
            {
                var fileName = @Request.Params.Get("file");

                filePath = Path.Combine(Server.MapPath("~/Files/" + fileName));
                userID = @Request.Params.Get("userID");
            }

            if ((!string.IsNullOrEmpty(filePath) && !filePath.Equals("")))
            {
                mainRHub.NotifyByID("<h3>Обработка</h3><div class='notRow'>Обработка данных начата</div>", userID);

                using (var stream = System.IO.File.OpenRead(filePath))
                using (var reader = new StreamReader(stream))
                {
                    var parsed = parser.Parse(reader, ' ', '\n');
                    List<List<string>> data = parsed.Cast<List<string>>().ToList();
                    var dataCount = data.Count();
                    mainRHub.processProgress(0, userID);
                    var elemList = new List<double>();
                    var json = new Dictionary<string, string>();

                    elemList = parceData(filePath, userID, "secondVal");

                    double sMean = 0.0;
                    sMean = sampleMean<double>(elemList);
                    mainRHub.processProgress(47.5, userID);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Среднее значение");
                    json.Add("value", sMean.ToString("#0.00000"));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);
                    //                Thread.Sleep(50);

                    double dispersion = 0.0;
                    dispersion = getDispersion(elemList, sMean);
                    mainRHub.processProgress(54.5, userID);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Дисперсия");
                    json.Add("value", dispersion.ToString("#0.00000"));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

                    List<double> kValuesList = new List<double>();
                    kValuesList = getKValuesList(elemList, sMean);
                    mainRHub.processProgress(62, userID);
                    json = new Dictionary<string, string>();
                    json.Add("message", "Табуляция Kx");
                    json.Add("value", string.Join("<br />", kValuesList));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

                    mainRHub.processProgress(69.5, userID);
                    sendDataOfCharts(kValuesList, "Оценка автокорреляционной функции", "Kx", userID, null, "line");

                    int So = elemList.Count / 10;
                    json = new Dictionary<string, string>();
                    mainRHub.processProgress(77.0, userID);
                    json.Add("message", "Ширина To корреляционных окон Бартлета и Хеминга и m<br /><br />(To было выбрано в размере 10%  от размера выборки)");
                    json.Add("value", string.Format("{0}, {1}", So, (elemList.Count / So).ToString("#")));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

                    List<double> firstVal = parceData(filePath, userID, "firstVal");
                    double dt = firstVal.ElementAt(1) - firstVal.ElementAt(0);
                    List<double> Sxb = new List<double>();
                    List<double> Sxn = new List<double>();
                    Tuple<List<double>, List<double>> tabulated = tabulateSx(userID, 0, 50, 0.001, int.Parse((elemList.Count / So).ToString()), So, dt, kValuesList);
                    Sxb = tabulated.Item1;
                    Sxn = tabulated.Item2;
                    json = new Dictionary<string, string>();
                    mainRHub.processProgress(84.5, userID);
                    json.Add("message", "Sx Барклет<br /><br />Sx Хамминг");
                    json.Add("value", string.Format("{0} элементов<br /><br />{1} элементов", Sxb.Count, Sxn.Count));
                    mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);


                    mainRHub.processProgress(92.0, userID);
                    sendDataOfCharts(Sxb, "Оценка спектральной плотности по окнам Барклета", "Sxb", userID, null, "line");
                    
                    sendDataOfCharts(Sxb, "Оценка спектральной плотности по окнам Хамминга", "Sxn", userID, null, "line");
                    mainRHub.processProgress(100, userID);

                    json = new Dictionary<string, string>();
                    json.Add("title", "Выводы");
                    mainRHub.sendConclusionOfCalculating(JsonConvert.SerializeObject(json), userID, mainRHub.typeOfSendingString.rowsTitle);

                    json = new Dictionary<string, string>();
                    json.Add("conclusion", "В данной работе мы приобрели практические навыки оценки корреляционной функции и спектральной" + 
                        "плотности стационарного процесса с использованием ЭВМ и построили оценки данных функций по экспериментальным данным.");
                    
                    mainRHub.sendConclusionOfCalculating(JsonConvert.SerializeObject(json), userID);

                    mainRHub.NotifyByID(String.Format("<h3>Обработка</h3><div class='notRow'>Обработка окончена</div>" + 
                        "<div class='notRow'>Обработано: {0} записей</div>", elemList.Count), userID);
                }
            }
            return PartialView();
        }

        public ActionResult oedCS3(string filePath = "", string userID = "")
        {
            if (string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(@Request.Cookies.Get("file").Value))
            {
                var fileName = @Request.Params.Get("file");

                filePath = Path.Combine(Server.MapPath("~/Files/" + fileName));
                userID = @Request.Params.Get("userID");
            }

            if ((!string.IsNullOrEmpty(filePath) && !filePath.Equals("")))
            {
                mainRHub.NotifyByID("<h3>Обработка</h3><div class='notRow'>Обработка данных начата</div>", userID);

                using (var stream = System.IO.File.OpenRead(filePath))
                using (var reader = new StreamReader(stream))
                {
                    var parsed = parser.Parse(reader, ' ', '\n');
                    List<List<string>> data = parsed.Cast<List<string>>().ToList();
                    var dataCount = data.Count();
                    mainRHub.processProgress(0, userID);
                    var elemList = new List<double>();
                    var json = new Dictionary<string, string>();

                    elemList = parceData(filePath, userID, "secondVal");


                }
            }
                    return PartialView();
        }

        private double sampleMean<T>(List<T> data)
        {
            double result = 0.0;
            double temp = 0.0;
            if (double.TryParse(data.First().ToString(), out temp))
            {
                foreach (var item in data)
                {
                    Double.TryParse(item.ToString(), out temp);
                    result += temp;
                }
                result = result / data.Count;
            }
            return result;
        }

        private double selectiveQuadratic<T>(List<T> data)
        {
            double result = 0.0;
            double temp = 0.0;
            double sampleMean = sampleMean<T>(data);
            if (double.TryParse(data.First().ToString(), out temp) && sampleMean != double.NaN)
            {
                foreach (var item in data)
                {
                    Double.TryParse(item.ToString(), out temp);
                    result += Math.Pow(temp - sampleMean, 2);
                }
                result = result / (data.Count - 1);
            }
            return result;
        }

        private double pointEstimateOfSelectiveQuadratic(double selectiveQuadratic)
        {
            double result = 0.0;
            if (selectiveQuadratic > 0.0)
            {
                result = Math.Sqrt(selectiveQuadratic);
            }
            return result;
        }

        private double EstimateOfSelectiveQuadratic(double selectiveQuadratic, int n)
        {
            double result = 0.0;
            double pointEstimateOfSQ = pointEstimateOfSelectiveQuadratic(selectiveQuadratic);
            if (pointEstimateOfSQ > 0.0)
            {
                result = pointEstimateOfSQ / Math.Sqrt(n);
            }
            return result;
        }

        private int intervalsCount(long dataCount) {
            return Convert.ToInt32(3.3 * Math.Log10(dataCount) + 1);
        }

        private double stepSize(List<dataType> data, string valueColumn, int iCount) {
            double result = 0.0;
            var minAmdMaxValues = minMaxValue(data, valueColumn);
            double L = minAmdMaxValues.Item2 - minAmdMaxValues.Item1;
            result = L / Convert.ToDouble(iCount);

            return result;
        }

        private Tuple<double, double> minMaxValue(List<dataType> data, string valueColumn) {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            double min = 0.0;
            double max = 0.0;

            Thread Min = new Thread(() => { dataDB.ForEach(x => min = Math.Min(min, 
                Convert.ToDouble(x.GetType().GetProperty(valueColumn, bindFlags).GetValue(x)))); });
            Thread Max = new Thread(() => { dataDB.ForEach(x => max = Math.Max(max, 
                Convert.ToDouble(x.GetType().GetProperty(valueColumn, bindFlags).GetValue(x)))); });

            Min.Start();
            Max.Start();

            Min.Join();
            Max.Join();

            return new Tuple<double, double>(min, max);
        }

        private List<double> getIntervals(double minVal,int iCount, double sSize) {
            List<double> result = new List<double>();

            result.Add(minVal);
            for (int i = 0; i < iCount; i++)
            {
                result.Add(result.ElementAt(i) + sSize);
            }

            return result;
        }

        private List<string> getIntervalsToString(List<double> intervals) {
            List<string> result = new List<string>();
            int i = 0;

            foreach (var item in intervals.Skip(1))
            {
                result.Add(string.Format("({0}) — ({1})", intervals.ElementAt(i).ToString("#0.0000"), item > 0 ? item.ToString("#0.0000") : item.ToString("#0.0000")));
                i++;
            }

            return result;
        }

        private List<int> getChartIntervalsElementsCount(List<double> intervals, List<double> elemList)
        {
            List<int> result = new List<int>();
            List<double> tempList = elemList;
            double intervalStartAt = intervals.First();
            int j = 0;

            foreach (var interval in intervals.Skip(1))
            {
                foreach (var elem in tempList)
                {
                    if (elem >= intervalStartAt && elem < interval)
                    {
                        j++;
                    }
                }
                intervalStartAt = interval;
                result.Add(j);
                j = 0;
            }
            return result;
        }

        private Tuple<double, double> hiSquared(List<double> intervals, List<int> chartIntervalsElementsCount, double sampleMean, 
            double selectiveQuadratic, string userID, int intervalsCount, int intervalsPerThread = 3)
        {
            int i = 0;
            bool isEndOfLoop = false;
            List<Task<Tuple<double, double>>> pOfi = new List<Task<Tuple<double, double>>>();
            Tuple<double, double> result = new Tuple<double, double>(0,0);

            //            Hubs.mainRHub.NotifyAll(pOfi.Length.ToString());
            while (i < intervals.Count && !isEndOfLoop)
            {
                if (intervalsPerThread + i >= intervals.Count)
                {
                    if (i % intervalsPerThread == 0 && intervalsPerThread % 2 != 0)
                    {
                        break;
                    }
                    intervalsPerThread = intervals.Count - i - 1;
                    isEndOfLoop = true;
                }

                var tempI = i;
                var countOfElementsInIntervalForOneThread = chartIntervalsElementsCount.GetRange(tempI, intervalsPerThread);
                var intervalsForOneThread = intervals.GetRange(tempI, intervalsPerThread);
                pOfi.Add(Task.Factory.StartNew<Tuple<double, double>>(() => 
                    hiSquaredinInterval(countOfElementsInIntervalForOneThread, intervalsForOneThread, tempI, intervalsPerThread, sampleMean, selectiveQuadratic, intervalsCount)
                ));

                i += intervalsPerThread;
            }

            try
            {
                Task.WaitAll(pOfi.ToArray());
            }
            catch (AggregateException ae) {
                string error = string.Empty;
                foreach (var e in ae.InnerExceptions)
                {
                    error += ' ' + e.Message;
                }
                mainRHub.NotifyByID(error, userID);
            }

            foreach (var thread in pOfi)
            {
                result = new Tuple<double, double>(result.Item1 + thread.Result.Item1, thread.Result.Item2);
            }

            return result;
        }

        private Tuple<double,double> hiSquaredinInterval(List<int> elementsInInterval, List<double> intervals, int elemStart, 
            int elemToCalc, double sampleMean, double selectiveQuadratic, int intervalsCount)
        {
            double HiSquaredVisible = 0.0;
            double HiSquaredCritical = 0.0;
            int i = 0;
            List<Task<double>> threads = new List<Task<double>>();

            for (i = 0; i < elemToCalc - 1; i++)
            {
                Tuple<double, double> interval = new Tuple<double, double>(intervals.ElementAt(i), intervals.ElementAt(i + 1));
                int elementsCountInInterval = elementsInInterval.ElementAt(i);

                threads.Add(Task.Factory.StartNew<double>(() => HiSquared.calcVisible(
                    interval, elementsCountInInterval, intervals.Count, sampleMean, selectiveQuadratic
                )));
//                threads.Add(Task.Factory.StartNew<double>(() => HiSquared.calcCritical(dataCount, sampleMean)));

                try
                {
                    Task.WaitAll(threads.ToArray());
                }
                catch (AggregateException ae)
                {
                    throw ae;
                }

                HiSquaredVisible += threads.ElementAt(0).Result;

                threads.Clear();
            }
            var critThread = new Task<double>(() => HiSquared.calcCritical(intervalsCount, sampleMean));
            critThread.Start();
            critThread.Wait();
            HiSquaredCritical = critThread.Result;
            /*
                        var json = new Dictionary<string, string>();
                        json.Add("message", "Поток " + Thread.CurrentThread.ManagedThreadId.ToString());
                        json.Add("value", "Value " + result.ToString() + "<br />array:" + string.Join(", ", data));
                        Hubs.mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), "gfdh-ag3f-agr3-34fg-sdaeatgfbfad");
            */
            return new Tuple<double, double>(HiSquaredVisible, HiSquaredCritical);
        }
        private double expectedValue<T>(List<T> elemList) {
            double result = 0.0;
            double temp = 0.0;
            if (double.TryParse(elemList.First().ToString(), out temp))
            {
                foreach (var item in elemList)
                {
                    Double.TryParse(item.ToString(), out temp);
                    result += temp;
                }
                result = result / elemList.Count;
            }
            return result;
        }

        private double getDispersion(List<double> elemList, double sampleMean) {
            double result = 0.0;

            foreach (var item in elemList)
            {
                result += Math.Pow(item - sampleMean, 2);
            }
            result /= elemList.Count;
            return result;
        }

        private List<double> getKValuesList(List<double> elemList, double sampleMean) {
            List<double> result = new List<double>();
            int j = 0;

            foreach (var item in elemList)
            {
                result.Add(getKx(elemList, j, sampleMean));
                j++;
            }

            return result;
        }

        private double getKx(List<double> elemList, int elemPos, double sampleMean) {
            double result = 0.0;

            for (int i = 0; i < elemList.Count - elemPos; i++)
            {
                result += (elemList.ElementAt(i) - sampleMean) * (elemList.ElementAt(i + elemPos) - sampleMean);
            }

            result = result / (elemList.Count - elemPos);
            return result;
        }

        private Tuple<List<double>, List<double>> tabulateSx(string userID, double a, double b, double step, int m, int To, double dt, List<double> kValuesList) {
            Tuple<List<double>, List<double>> result = new Tuple<List<double>, List<double>>(new List<double>(), new List<double>());
            List<Task<List<double>>> sXFunc = new List<Task<List<double>>>();

            try
            {
                sXFunc.Add(Task.Factory.StartNew(() => sXBartlet(userID, a, b, step, m, To, dt, kValuesList)));
                sXFunc.Add(Task.Factory.StartNew(() => sXHamming(userID, a, b, step, m, To, dt, kValuesList)));

                Task.WaitAll(sXFunc.ToArray());

                result = new Tuple<List<double>, List<double>>(sXFunc.ElementAt(0).Result, sXFunc.ElementAt(1).Result);
            }
            catch (AggregateException ae)
            {
                string error = string.Empty;
                foreach (var e in ae.InnerExceptions)
                {
                    error += ' ' + e.Message;
                }
                mainRHub.NotifyByID(error, userID);
            }

            return result;
        }

        private List<double> sXBartlet(string userID, double a, double b, double step, int m, int To, double dt, List<double> kValuesList)
        {
            List<double> result = new List<double>();
            double res = 0.0;

            for (double i = a; i <= b; i += step)
            {
                res = 0.0;
                for (int j = 1; j <= m - 1; j++)
                {
                    res += bartletWindow(j * dt, To) * kValuesList.ElementAt(j) * Math.Cos(j * i * Math.PI / m);
                }
                res = 2 * res + bartletWindow(0, To) * kValuesList.ElementAt(0);
                res = res * dt / Math.PI;
                
                result.Add(res);
            }

            return result;
        }

        private double bartletWindow(double tau, int To) {
            if (Math.Abs(tau) <= To)
            {
                return 1.0 - Math.Abs(tau) / To;
            }
            else
            {
                return 0.0;
            }
        }

        private List<double> sXHamming(string userID, double a, double b, double step, int m, int To, double dt, List<double> kValuesList)
        {
            List<double> result = new List<double>();
            double res = 0.0;

            for (double i = a; i <= b; i += step)
            {
                res = 0.0;
                for (int j = 1; j <= m - 1; j++)
                {
                    res += hammingWindow(j * dt, To) * kValuesList.ElementAt(j) * Math.Cos(j * (m / 2) * Math.PI / m);
                }
                res = 2 * res + hammingWindow(0, To) * kValuesList.ElementAt(0);
                res = res * dt / Math.PI;

                result.Add(res);
            }

            return result;
        }

        private double hammingWindow(double tau, int To)
        {
            if (Math.Abs(tau) <= To)
            {
                return 0.54 + 0.46 * Math.Cos(Math.PI * tau / To);
            }
            else
            {
                return 0.0;
            }
        }

        private List<int> NumberOfRepetitions(IEnumerable<double> data) {
            List<int> result = new List<int>();
            int count = 0;

            for (int i = 0; i < data.Count(); i++)
            {
                count = 0;
                for (int j = 0; j < data.Count(); j++)
                {
                    if (data.ElementAt(i) == data.ElementAt(j))
                        count++;
                }
                result.Add(count);
            }

            return result;
        }

        private void sendDataOfCharts<T>(List<T> data, string title, string labelTitle, string userID, 
            List<string> titles = null, string chartType = "column")
        {
            List<chartData> dataList = new List<chartData>();
            bool isSingleTitle = titles == null ? true : false;
            bool isAllTitlsExists = (isSingleTitle.Equals(false) && data.Count.Equals(titles.Count)) ? true : false;
            int i = 1;

            foreach (var item in data)
            {
                if (isSingleTitle)
                {
                    dataList.Add(new chartData(Convert.ToDouble(item), string.Format("{0} {1}", labelTitle, i)));
                }
                else
                {
                    if (!isAllTitlsExists && i >= titles.Count)
                    {
                        dataList.Add(new chartData(Convert.ToDouble(item), string.Format("{0} {1}", labelTitle, i)));
                    }
                    else
                    {
                        dataList.Add(new chartData(Convert.ToDouble(item), titles.ElementAt(i - 1).ToString()));
                    }
                }
                i++;
            }

            mainRHub.addChart(JsonConvert.SerializeObject(dataList), title, userID, chartType);
        }
    }
}