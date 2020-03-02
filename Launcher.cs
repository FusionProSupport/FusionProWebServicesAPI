using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*      Change the using line below to point to the web service reference in your project
*/
using FPWebServicesDemo.FusionProAPI;

namespace FPWebServicesDemo
{

    class Launcher
    {
        // Launch a job with the specified input file, template and group, to the specified output file. It is hardcoded to PDF. 
        // Optionally wait until the job is complete before returning.
        public static string ComposeJob(string TemplateName, string GroupName, string InputFilePath, string OutputFolder, bool WaitUntilFinished)
        {
            CompositionRequest request = new CompositionRequest();
            CreateCompositionResponse resp = new CreateCompositionResponse();
            request.GroupName = GroupName;
            request.TemplateName = TemplateName;
            request.Options = new JobOptions();
            request.Options.OutputFormat = OutputFormat.PDF;
            request.Options.UseImposition = true;

            try
            {
                FPQueueWCFServiceClient myFPWcfService = new FPQueueWCFServiceClient();
                
                request.Options.OutputFolder = OutputFolder;
                /* Code below is optional to output every record to its own file. 
                request.Options.NamedSettings = new KeyValue[]
                {
                    new KeyValue { Name = "OutputSource", Value = "bychunk" },
                    new KeyValue { Name = "RecordsPerChunk", Value = settings.RecordsPerFile.ToString()},
                    new KeyValue { Name = "ReducedRecordsForPreview", Value = "6" }, // whatever you want here
                };  */

                resp = myFPWcfService.CreateCompositionSession(request);
                

                if (!String.IsNullOrEmpty(resp.Message))
                    return "Failed to create composition session: " + resp.Message;

                if (!String.IsNullOrEmpty(InputFilePath))
                {
                    AddCompositionComponentRequest compReq = new AddCompositionComponentRequest();
                    compReq.Type = FileType.InputData;
                    compReq.RemoteFilePath = InputFilePath;
                    compReq.CompositionID = resp.CompositionID;
                    compReq.ComponentType = FPCompositionComponentType.FromFile;
                    AddCompositionComponentResponse addResp = new AddCompositionComponentResponse();


                    addResp = myFPWcfService.AddCompositionFile(compReq);
                    if (!String.IsNullOrEmpty(addResp.Message))
                        return "Failed to add input file for composition " + resp.CompositionID + ": " + addResp.Message;
                }

                StartCompositionRequest startReq = new StartCompositionRequest();
                StartCompositionResponse startResp = new StartCompositionResponse();

                startReq.CompositionID = resp.CompositionID;

                startResp = myFPWcfService.StartCompositionFromSession(startReq);
                if (!String.IsNullOrEmpty(startResp.Message))
                    return "Failed to start composition " + resp.CompositionID + ": " + startResp.Message;

                if (WaitUntilFinished)
                {
                    while (true)
                    {
                        CompositionFilePathURLRequest compFilePathReq = new CompositionFilePathURLRequest();
                        compFilePathReq.CompositionID = resp.CompositionID;

                        CompositionStatus status = myFPWcfService.CheckCompositionStatus(compFilePathReq);
                        if (!String.IsNullOrEmpty(status.Message))
                            return "Failed to add get status of composition " + resp.CompositionID + ": " + status.Message;

                        switch (status.JobStatus)
                        {
                            case JobStatus.Cancelled:
                            case JobStatus.Failed:
                            case JobStatus.None:
                                return "Composition " + resp.CompositionID + ": " + status.JobStatus.ToString();

                            case JobStatus.Queueing:
                            case JobStatus.InProcess:
                                //continue;
                                // Here you can check status.PercentComplete for updates.
                                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                                break;

                            case JobStatus.Done:
                                if (status.ReturnCode != 0)
                                    return "Composition " + resp.CompositionID + " returned error: " + status.ReturnCode;

                                break;
                        }
                    }
                }

                return ""; // no error
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}