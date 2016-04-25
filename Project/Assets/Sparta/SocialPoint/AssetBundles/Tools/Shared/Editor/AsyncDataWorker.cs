using UnityEngine;
using System;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared
{
	public class AsyncDataWorker
	{
		public enum States { idle, done, faulted };

	    private System.ComponentModel.BackgroundWorker Worker {get; set;}
	    private object _lock = new object();
	    private object _result;

	    public AsyncDataWorker()
	    {
	        Init();
	    }
	    
	    private bool Init()
	    {
	        if (Worker != null && Worker.IsBusy) return false;
	        
	        Worker = new System.ComponentModel.BackgroundWorker()
	                        {
	                            WorkerSupportsCancellation = true,
	                            WorkerReportsProgress = true
	                        };
	        
	        _result = null;
	        
	        return true;
	    }
	    
	    public bool Cancel()
	    {
	        if (!Worker.IsBusy) return false;
	       
	        Worker.CancelAsync();
	        
	        return true;    
	    }

		public void Destroy()
		{
			
		}

	    public bool RunAsync(Func<object> action, object defaultValue = null)
	    {
			if (Worker.IsBusy) return false;
	        
	        _result = defaultValue;
	        
	        Worker.DoWork += (sender, args) => {
	            var bgWorker = sender as System.ComponentModel.BackgroundWorker;
	            
	            if (bgWorker.CancellationPending == true) 
	            {
	                args.Cancel = true;
	                return;
	            }
	            try {
	            	args.Result = action();
				}
				catch (Exception e)
				{
					args.Result = e;
				}
	        };

	        
	        Worker.RunWorkerCompleted += (sender, e) => {
	            lock(_lock)
	            {
	                _result = e.Result;
	            }
	        };
	        
	        // Start the bg job 
	        Worker.RunWorkerAsync();

	        return true;
	    }

	    public bool IsRunning
	    {
	        get
	        {
	            return Worker.IsBusy;
	        }    
	    }
	    
	    public T GetResult<T>() where T : class
	    {
	        T resultTyped = default(T);
	        
	        lock(_lock)
	        {
	            resultTyped = (T)_result;    
	        }
	        
	        return resultTyped;
	        
	    }
	}

	public class AsyncDataWorkerSet
	{
	    private Dictionary<string, AsyncDataWorker> _backgroundWorkersData {get;set;}

	    public AsyncDataWorkerSet()
	    {
	        _backgroundWorkersData = new Dictionary<string, AsyncDataWorker>();
	    }
	    
	    public AsyncDataWorker this[string index]
	    {
	        get
	        {
	            if (!_backgroundWorkersData.ContainsKey(index))
	            {
	                this[index] = new AsyncDataWorker();
	            }
	            
	            return _backgroundWorkersData[index];
	        }
	        private set
	        {
	            _backgroundWorkersData[index] = value;
	        }
	    }    
	}
}
