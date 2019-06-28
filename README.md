
# ESRI-Mitac

> HoloLens APP for collaboration in urban designing

Urban designing is about shaping the physical features of cities, towns, and villages and planning for the provision of municipal services to residents and visitors. Rather than proposing traditional on-side field research before the real designing with manual drawing and measurement on papers, modern designing can be easily tasked on a digital map from which multiple kinds of geophysical information can be retrieved quickly and accurately. We had a touch table application which can create a 2-dimentional (2D) map of a certain district and generate a bunch of buildings as well as public facilities attached to the map layout for the urban design. However, the 2D displayed basemap (e.g. Esri ArcGIS Maps) has limitation for only showing a top-down view of the planned area, while a more immersive experience could be provided if the map can be displayed in 3D space. User will be given additional degrees of freedom to explorer the design in a 3D immersive display. Based on this idea, We built this AR application which simultaneously visualizes the map as the touch table shows and provides collaboration between designers on the touch table side and on HoloLens side. The HoloLens user is able to see the synchronized map and designing scheme even when he is in a remote work place and still can discuss the solution with the touch table designer.

# Introduction for the HoloLens App

This application is designed for users who are experts in the urban design domain. To be more specific, an expert can browse a 3D virtual Esri map to see a real-time design via a Microsoft HoloLens. When designers carry out the design with the touch table application, while the expert can remotely supervise the whole procedure.

> This HoloLens App is formed with four major modules:

**1, ESRI map rendering module**
The App accesses to the ESRI's map service to download the map tiles, terrain height, and imagery. All these data will be set into the terrain data and form a 3D terrain map. The map coordinates are synchronized with the touch table application. So we can visualize the same location on the both sides. The App adopts the spatial mapping feature of HoloLens to scan the whole space and calculate a flat surface where we can attach the map model. We also provide several functions for handling the map including: multi-direction movement, zoom in/out, 2D-3D switch, street/satellite map switch, etc.

**2, Facilities Operation and Synchronization module**
The buildings and facilities on the map will be synchronized with those on the touch table map. On the touch table, when the designer put down a pre-designed building module, the attributes of the module such as name, coordinates, rotation will be updated into the database server. Simultaneously, the HoloLens App will download the corresponding building from the server and adjust its attributes according to the record in the database. To assist the design, the HoloLens App also provides functions for manipulating the building models including : rotating, re-locating, duplication. Once the buildings are manipulated, all their attributes will be updated into the database server and feed back to the touch table. So users on both sides will see the same modification.

**3, Video Conference module**
For the convenience during the collaboration, we integrated a video conference module into our HoloLens App. This module is implemented based on the WebRTC library. The user can call the touch table user on the HoloLens. Once the connection is established, they can have share the view with each other and discuss via the video and audio channels that will make the communication more natural and efficient.

**4, Database and coordination server**
We built a LAMP structure server for modules management and data storage. All available pre-designed building modules will be stored in the server, the clients can fetch the required modules from the server rather than storing them locally. It can save lots of space on the local device. The database is responsible for the data coordination. All information of map, building, facilities will be stored in the database. The HoloLens and touch table applications share and update the data through the database for a synchronized visualization. This back-end is very flexible to deploy. We can deploy it on local machine, virtual machine/docker or cloud platform such as AWS and Azure.

**The GitHub repository:**
*(https://github.com/NathanSun1981/ESRI-APP-Competition)*

**A video demo can be checked from the link below:**
*https://drive.google.com/file/d/1T57VUWhBEHn68JYZzMUzrviTIxz798Az/view?usp=sharing*

# How to Run the Application

1. Check out or download the entire repository from GitHub and install all the tools for the HoloLens environment. Make sure you have the latest update for Visual Studio 2017 and Unity 2017.1.4 LTS and install the "Windows 10 SDK". More detailed instructions can be found here:
https://docs.microsoft.com/en-us/windows/mixed-reality/using-visual-studio

2. Once the environment was setup successfully (might take a few hours to install the tools), re-build the project in Unity. Once success, open 'ESRI.sln' in Microsoft Visual Studio 2017. You can either choose to run the application from the real HoloLens device if you have one or choose to run on the HoloLens Emulator.

3. The server is public without any firewall issues, the App can also access our host off campus.