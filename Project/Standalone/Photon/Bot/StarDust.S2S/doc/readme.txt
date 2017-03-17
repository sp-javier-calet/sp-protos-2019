Stardust.S2S - Photon Server-To-Server Performance Testing
-----------------------------------------------------------

Read introduction here: 
http://doc.photonengine.com/en/onpremise/current/reference/performance-tests

Run loadtest with the standard Photon SocketServer SDK: 
-----------------------------------------------------------

1. Start the "Loadbalancing" instance of the standard Photon SocketServer SDK.  

2. Modify /deploy/Stardust.S2S/bin/Stardust.S2S.Server.dll.config. 

Required settings: 
- ServerAddress
- Protocol
- Number of Clients / Games
- Unreliable/Reliable Data settigns (how often should RaiseEvent be called, and how much payload data should it contain) 

2. Start the Stardust.S2S instance - preferably on a different server than the Loadbalancing instance 

Once the Stardust.S2S instance is started, the test is started automatically: it creates the configured amount of connections to the target server, creates/joins games and calls RaiseEvent. 
The test run can be stopped by stopping the Photon Stardust.S2S instance. 

3. Check /deploy/log/StardustS2S.log for any errors

4. There are two ways to check current stats: 

a) Windows performance counter category "Photon: Stardust Cilent" 
b) Log file: /deploy/log/StardustS2SStats.log for current stats. 

Values: 
peers:  number of currently connected clients 
r-ev:  # of reliable events received
ur-ev: # of unreliable events received
r-op: # of reliable operations sent
ur-op: # of unreliable operations received
res: # of received operation responses
r-rtt: roundtrip time for reliable operations
ur-rtt: roundtrip time for unreliable operations
rtt: roundtrip time (overall) 
var: variance of roundtrip time  
flsh-rtt: roundtrip time for flush operations
flsh-ev: # of flush events received
flsh-op: # of flush operations received 


To modify the test code: 
-----------------------------------------------------------

1. Open /src-server/Stardust/Stardust.S2S.sln in Visual Studio

2. Make modifications to the sample code, according to your needs.  

3. Copy the output from /src-server/Stardust/Stardust.S2S/bin/ to /deploy/Stardust.S2S/bin

4. (Re)start Photon instance Stardust.S2S
