# VR Project Scripts
This project was created by Elijah Gulley

The main function of this project is the cutting algorithm seen within the CutManager script.
This algorithm is detailed below.

1. First, the user draws a line. The program checks when the mouse button is held down, and while it
   is, the mouse position is added to a LineRenderer object based on a certain timer and minimum distance. 
   Each position gets checked to make sure it is contained within the object using a RayCast function. 
   From the point in space, a ray is created and shot straight back into infinity. If the ray 
   intersects the object, then the point is valid. Once the user lets go of the mouse, the first 
   position is added back to the line to close the polygon. Then, the line can be simplyfied using
   a built-in function based on a certain tolerance. This allows more control over how many points 
   end up on the final polygon. Once the final line is finished, an event is triggered to let the 
   PathNodeCreator know when to start its functions.
2. The PathNodeCreator loops through the list of positions in the LineRenderer object and creates a
   PathNode object at each point. These object simply hold a Vector3 for the point in the world that 
   they represent. These are then used as vertices for the ear clipping algorithm. Once all PathNodes 
   are created, then another event is triggered so the CutManager can begin its tasks.
3. First, the script stores each Vector3 from the PathNodes in a list. It also stores a list of the indices, with 
   each index representing a certain vertex. Then, the ear clipping algorithm begins. 

      The ear clipping algorithm is very simple. First, it runs through each point and finds the maximum point in the x and the y.
      Then, it finds those points and draws triangles between each max and its adjacent points. It then removes each point from the 
      list and repeats until only three points are left. The last three points are then added to complete the polygon.

   The ear clipping algorithm creates triangles for one face of the object, so the points are added in reverse order to
   the triangles list in order to render the back face. Then, each side face is rendered by looping through all the vertices
   and adding the two triangles that will make up the rectangle that forms each face. Once these side faces are all added, 
   the vertices are each added back in individually to make sure that no triangle shares a vertex. This is needed to ensure
   the lighting on the object works well within Unity.
   
