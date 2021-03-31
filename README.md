# Sheduler

Sheduler to run a queue asyncronously of synchronous tasks secuentially. 

Divided in two components, one is a .Net Framework class library and the other 
its corresponding NUnit3-Test project.

# You will find

This Sheduler can be added to a GUI (Forms, ...) and interact properly with the controls because can be runnend in the same synchronization Context.

1. Cancel concatenated Task using CancellationTokenSource and CancellationToken
2. NUnit test provided. Also to check how to use the SequentialQueueSheduler.
