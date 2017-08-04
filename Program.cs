using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parameter_Sweep_Data_Tool
{
    class FileData
    {
        public List<float> spread = new List<float>();
        public List<int> invasion = new List<int>();
    }

    class ParameterSet
    {
        public List<FileData> repeats = new List<FileData>();
        public List<float> meanSpread = new List<float>();
        public List<float> sdSpread = new List<float>();
    }

    class Program
    {
        static void Main(string[] args)
        {
            string folder = @"C:\Users\Tim\Dropbox\Phd\Publication\Field Spread\parameter sweep data";
            string[] files = System.IO.Directory.GetFiles(folder);

            Dictionary<String, ParameterSet> data;
            List<String> filesThatShouldExist = new List<string>();

            double[] quiescenceMultipliers = { 1.0, 0.75, 0.5, 0.25, 0.1};
			double[] adhesionMultipliers = {1.0, 2.5, 5.0, 7.5, 10.0};
			double[] forceMultipliers = {1.0, 0.75, 0.5, 0.25, 0.1};

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        double[] mutation = { quiescenceMultipliers[i], adhesionMultipliers[j], forceMultipliers[k] };

                        string name;
                        name = "Q" + mutation[0].ToString() + "AF" + mutation[1].ToString() + "CF" + mutation[2].ToString();
                        filesThatShouldExist.Add(name);

                    }
                }
            }

            data = new Dictionary<string, ParameterSet>();

            foreach (string filename in files)
            {
                string name = System.IO.Path.GetFileName(filename);
                name = name.Remove(name.Length - 8);

                if (!data.ContainsKey(name))
                {
                    data.Add(name, new ParameterSet());
                }
                FileData fileData = LoadFileData(filename);

                data[name].repeats.Add(fileData);
            }

            List<string> missingFiles = new List<string>();
            Dictionary<string, int> missingRepeats = new Dictionary<string,int>();
            Dictionary<string, int> shortFiles = new Dictionary<string, int>();
            Dictionary<string, List<int>> invasionData = new Dictionary<string,List<int>>();

            foreach (string file in filesThatShouldExist)
            {
                if (!data.ContainsKey(file))
                {
                    missingFiles.Add(file);
                }
                else
                {
                    if (data[file].repeats.Count < 5)
                    {
                        missingRepeats.Add(file, data[file].repeats.Count);
                    }
                }
            }
            foreach (string file in data.Keys)
            {
                foreach (FileData fd in data[file].repeats)
                {
                    if (fd.invasion.Count != 5000)
                    {
                        if (!shortFiles.ContainsKey(file))
                        {
                            shortFiles.Add(file, 0);
                        }
                        shortFiles[file]++;
                    }
                }

                DoStats(data[file]);
                ProcessInvasions(file, data[file], invasionData);
            }

            List<List<float>> formattedMeans = new List<List<float>>();
            List<List<float>> formattedSDs = new List<List<float>>();

            int totalCount = 0;
            foreach (string file in data.Keys)
            {
                ParameterSet set = data[file];
                totalCount = Math.Max(totalCount, set.meanSpread.Count);
            }

            for (int i = 0; i < totalCount; i++)
            {
                formattedMeans.Add(new List<float>());
                formattedSDs.Add(new List<float>());
            }

            foreach (string file in data.Keys)
            {
                ParameterSet set = data[file];
                if (set.meanSpread.Count == totalCount)
                {
                    for (int i = 0; i < totalCount; i++)
                    {
                        formattedMeans[i].Add(set.meanSpread[i]);
                        formattedSDs[i].Add(set.sdSpread[i]);
                    }
                }
            }

            System.IO.StreamWriter meanWriter = new System.IO.StreamWriter(folder + "/means.csv");
            System.IO.StreamWriter sdWriter = new System.IO.StreamWriter(folder + "/SDs.csv");
            System.IO.StreamWriter nameWriter = new System.IO.StreamWriter(folder + "/names.csv");

            foreach (string file in data.Keys)
            {
                nameWriter.Write(file);
                nameWriter.Write(',');
            }
            nameWriter.Write('\n');

            for (int i = 0; i < totalCount; i++)
            {
                for(int j = 0; j < formattedMeans[i].Count; j++)
                {
                    meanWriter.Write(formattedMeans[i][j]);
                    meanWriter.Write(',');
                    sdWriter.Write(formattedSDs[i][j]);
                    sdWriter.Write(',');
                }
                meanWriter.Write('\n');
                sdWriter.Write('\n');
            }

            int a = 0;
        }

        private static void DoStats(ParameterSet parameterSet)
        {
            int count = parameterSet.repeats.Count;
            
            for (int i = 0; i < parameterSet.repeats[0].spread.Count; i++)
            {
                int actualCount = 0;

                float mean = 0.0f;
                for (int j = 0; j < count; j++)
                {
                    if (parameterSet.repeats[j].spread.Count > i)
                    {
                        mean += parameterSet.repeats[j].spread[i];
                        actualCount++;
                    }
                }
                mean /= (float)actualCount;

                parameterSet.meanSpread.Add(mean);

                float variance = 0.0f;
                for (int j = 0; j < count; j++)
                {
                    if (parameterSet.repeats[j].spread.Count > i)
                    {
                        float valMinusMean = parameterSet.repeats[j].spread[i] - mean;
                        variance += valMinusMean * valMinusMean;
                    }
                }

                variance /= actualCount;
                parameterSet.sdSpread.Add((float)Math.Sqrt(variance));
            }
        }

        private static void ProcessInvasions(string file, ParameterSet parameterSet, Dictionary<string, List<int>> invasionData)
        {
            foreach (FileData fd in parameterSet.repeats)
            {
                int maxInvasion = 0;
                foreach (int inv in fd.invasion)
                {
                    if (inv > maxInvasion)
                    {
                        maxInvasion = inv;
                    }
                }
                if (maxInvasion != 0)
                {
                    if (!invasionData.ContainsKey(file))
                    {
                        invasionData.Add(file, new List<int>());
                    }
                    invasionData[file].Add(maxInvasion);
                }
            }
        }

        private static FileData LoadFileData(string filename)
        {
            FileData data = new FileData();
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);

            reader.ReadLine(); // discard first line

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] parts = line.Split(',');

                if (parts.Count() == 2)
                {
                    data.spread.Add(float.Parse(parts[0]));
                    data.invasion.Add(int.Parse(parts[1]));
                }
                else
                {
                    break;
                }
            }

            return data;
        }
    }
}
