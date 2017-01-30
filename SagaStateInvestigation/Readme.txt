This builds partly on the Rebus sabple for Request/Reply.

I suggest starting SagaStateTestApp and ExternalService application simultaneously
Set breakpoints in TestSaga at the handler for SomeReply at line where await DoStuffIfNotTimedout() is called and first line in the TimeOutMessage.
May need some tampering with the timeout/sleep settings...

Anyway, when I ran this as-is, I got to the SomeRequest-handler first which set the saga data state flag. After a while the timeout occurs and when the flag is checked it is 'false'.


BTW Started thinking when i wrote this :)
Maybe my problems would be resolved if I let the async operations that occur when SomeReply is received be handled by a message handler instead.
Could it be that the saga instance is somehow 'locked' by that handler until those operations are handled...


#UPDATE 
Added an alternative saga that probably solves my problem. Comment out the USE_ORIGINAL directive to use.