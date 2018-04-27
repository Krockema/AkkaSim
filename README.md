# AkkaSim
DES Simulation implementation, with pesimistic central clock


# Akka.Net
Based on my Master thesis with a kinda slow solution for a .Net based DES System and similar behave to the Aktor pattern the idea came across to give Akka.Net a try to build a superfast Aktor based discrete event simulation system. This is the outcome to this idea since i did not found any .Net based solutions that satisfies my requirements. Your welcome to improve and further develop the system.

# ToDo's
* Simulation interrupt
* Message Logging
* Wrapping Akka correctly that any user does not have to bother about the Akka.Net world nor is able to falsy use the DES.
* May implement diffrent a Distributed Clock like
  * https://en.wikipedia.org/wiki/Vector_clock or 
  * https://en.wikipedia.org/wiki/Lamport_timestamps
