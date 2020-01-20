using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using System.Data;


namespace Chad_Jasicki_DotNet2AppProject_CS
{
    class Program
    {
        // The string[] args are the command line arguments from  the properties page.
        static void Main(string[] args)
        {
            Int32 intIdx;
            String strFileName = "";
            string strErrorFileName = "";
            FileInfo fiData;
            StreamReader rdrData = null;
            string strRecord;
            String[] strFields;
            int iTotalWritten = 0;
            int i = 0;
            int intErrX = 0;

            //step 1 Check for no parameters
            if (args.Length < 1)
            {
                Console.WriteLine("No Parameter");
                Environment.Exit(-1);
            }

            //Step 2 Retrieve -F parameter
            for (intIdx = 0; intIdx < args.Length; intIdx++)
            {
                if (args[intIdx].StartsWith("-F")) //file to import data from
                {
                    strFileName = args[intIdx].Substring(2);
                }
                if (args[intIdx].StartsWith("-E"))//Error Log Name
                {
                    strErrorFileName = args[intIdx].Substring(2);
                }
            }
            // Step 3 Check contents of -F parameter
            if (strFileName.Trim().Length < 1)
            {
                //Write the error message to the onsole.
                Console.WriteLine("Missing file parameter");
                // if file for error log is supplied.
                if (!string.IsNullOrWhiteSpace(strErrorFileName))
                {
                    LogError("Missing File Parameter", strErrorFileName);
                }
                Environment.Exit(-1);
            }
            // Step 4: Check Existence of File
            fiData = new FileInfo(strFileName);
            if (!fiData.Exists)
            {
                Console.WriteLine("File " + strFileName + " does not exist", strErrorFileName);
                if (!String.IsNullOrWhiteSpace(strErrorFileName))
                {
                    LogError("File " + strFileName + " Does Not Exist", strErrorFileName);
                }
                Environment.Exit(-1);
            }
            // Step 5: Check file is not empty.
            if (fiData.Length < 1)
            {
                Console.WriteLine("File " + strFileName + " is empty");
                if (!String.IsNullOrWhiteSpace(strErrorFileName))
                {
                    LogError("File " + strFileName + " is empty", strErrorFileName);
                }
                Environment.Exit(-1);
            }
            // Step 6: Open file
            try
            {
                rdrData = new StreamReader(strFileName);
            }
            catch
            {
                Console.WriteLine("Error opening " + strFileName);
                if (!String.IsNullOrWhiteSpace(strErrorFileName))
                {
                    LogError("Error opening " + strFileName, strErrorFileName);
                }
                Environment.Exit(-1);
            }
            // Step 7: Read records
            while (rdrData.Peek() > 0) //Does more data exist in the file
            {
                i++;
                strRecord = rdrData.ReadLine();
                Console.WriteLine("Record Number: " + i + " = " + strRecord);

                //Step 8: Split fields
                strFields = strRecord.Split(",".ToCharArray());
                

                //Step 9: Validate fields (including count of fields
                if (strFields.Length > 5)
                {
                    intErrX++;
                    DateTime aDate = DateTime.Now;
                    LogError(aDate.ToString("dddd, dd MMMM yyyy HH:mm:ss") + " - Error: Not enough fields " + strFileName, strErrorFileName);
                    Console.WriteLine("Error: Too Many fields in record number " + i + " # of fields in record " + strFields.Length + " " + strRecord);
                }
                else if (strFields.Length < 5)
                {
                    intErrX++;
                    DateTime aDate = DateTime.Now;
                    LogError(aDate.ToString("dddd, dd MMMM yyyy HH:mm:ss") + " - Error: Too Many fields " + strFileName, strErrorFileName);
                    Console.WriteLine("Error: Missing fields in record number " + i + " # of fields in record " + strFields.Length + " " + strRecord);
                }
                else
                {
                    if (!IsValidRecord(strFields))
                    {
                        Console.WriteLine("Error: " + strRecord);
                        intErrX++;
                    }
                    else
                    {
                        //add DB
                        //Step 10: Insert record in database with validation from the clsDatabse class.
                        Boolean bolRetCode = false;
                        bolRetCode = clsDatabase.InsertEmployee(strFields[0].Replace("\"", ""), strFields[1].Replace("\"", ""), strFields[2].Replace("\"", ""), strFields[3].Replace("\"", ""), strFields[4].Replace("\"", ""));
                        Console.WriteLine("Field 1: " + strFields[0].Replace("\"", ""));
                        Console.WriteLine("Field 2: " + strFields[1].Replace("\"", ""));
                        Console.WriteLine("Field 3: " + strFields[2].Replace("\"", ""));
                        Console.WriteLine("Field 0: " + strFields[3].Replace("\"", ""));

                        if (!bolRetCode)
                        {
                            Console.WriteLine("Try again, no records writen to database");
                        }
                        else
                        {
                            Console.WriteLine("Employee was written to Database");
                            iTotalWritten++;
                        }

                    }
                }
            }
            //Step11: Close file
            rdrData.Close();

            //Step12: Report results
            Console.WriteLine(". ");
            Console.WriteLine(".. ");
            Console.WriteLine("... ");
            Console.WriteLine("Total Emp Added to Database " + iTotalWritten);
            Console.WriteLine("Emp Records not added " + intErrX);
            Console.WriteLine("Press enter to close program");
            Console.ReadKey(); // Pause
            Environment.Exit(0); // Close application.
        }
        private static Boolean IsValidRecord(String[] strCheck)
        {
            //Check for valid data, checking for no SSAN and adding record if there isn't any match SSAN in DB
            Boolean blnOK = true;            
            int icount = 0;
            blnOK = true;
            foreach (string x in strCheck)
            {
                icount++;  //First Name
                if (icount == 1)
                {
                }
                else if (icount == 2)  // Middle can only be one char
                {
                   if(x.Length == 1 || x.Length == 0)
                    {
                        blnOK = true;
                    }
                   else
                    {
                        blnOK = false;
                        Console.WriteLine("Error: Can only have middle initial, one chr");
                    }

                }
                else if (icount == 3) //last
                {

                }
                else if (icount == 4) //SSAN
                {
                    if (int.TryParse(x, out int j) && x.Length == 9)
                    {
                        Console.WriteLine("SSAN is formated correctly and all numbers");

                        DataSet dscheck;
                        dscheck = clsDatabase.EmployeeExists(x);
                        if (dscheck.Tables[0].Rows.Count != 0)
                        {
                            Console.WriteLine("SSAN already used...................");

                            blnOK = false;
                        }
                        dscheck.Dispose();
                    }
                    else
                    {
                        Console.WriteLine("Error: SSAN is Bad........");
                        blnOK = false;
                    }
                }
                else if (icount == 5) // // checks for resonable payrate
                {
                    if (double.TryParse(x, out double j))
                    {
                        if (j > 0 && j < 800)
                        {
                            Console.WriteLine("Valid PayRate");
                        }
                        else

                        {
                            Console.WriteLine("PayRate was invalid");
                            blnOK = false;
                        }
                    }
                    else
                    {
                        blnOK = false;
                    }
                }
            }

            //Two methods to test the data are shown.
            if (strCheck[0].Replace("\"", "").Trim().Length < 1)
            {
                blnOK = false;
            }
            if (String.IsNullOrWhiteSpace(strCheck[0].Replace("\"", "")))
            {
                blnOK = false;
            }
            return blnOK;
        }

        private static Int32 LogError(String strMessage, String strErrorFileName)
        {
            Int32 intRetVal = 0;

            // Create a writer and open the file:
            StreamWriter log;
            try
            {
                if (!File.Exists(strErrorFileName))
                {
                    // if file does not exist, create file
                    log = new StreamWriter(strErrorFileName);
                }
                else
                {
                    // if file exists, append to file
                    log = File.AppendText(strErrorFileName);
                }
                // Write to the file
                log.WriteLine(DateTime.Now);
                log.WriteLine(strMessage);
                log.WriteLine();

                //close the file
                log.Close();
            }
            catch
            {
                intRetVal = -1;
            }
            return intRetVal;
        }
    }
}
