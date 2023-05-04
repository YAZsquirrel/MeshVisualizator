# Mesh Visualisator 
## About 
Mesh Visualizator is a simple 2D visualization tool for two-dimensional FEM, FDM, splines, and vector FEM solutions 

###### Made with OpenTK and WPF on C#
## Examples

![Example](/RMrs/example_s1.png "File inputs") 

![Example](/RMrs/example_s2.png "File inputs")  

![Example](/RMrs/example_s3.png "File inputs")

![Example](/RMrs/example_v.png "File inputs")

![Example](/RMrs/example_v2.png "File inputs")

# Instruction 
## Inputs
Input your files in these labels (via drag'n'drop or by menu).

![Files](/RMrs/files.png "File inputs")

* Vertex file format
    > **Format:** X  Y   
    *Where* [X, Y] are coordinates of n-th vertex

    #### **Example:**   
    >  -1.0 -1.0   
    >  1.0 -1.0  
    >  -1.0 1.0  
    >  1.0 1.0  
* #### Elements file format
    * Triangle
        > **Format:** p0 p1 p2   
        > Where [p0, p1, p2] are global numbers of vertecies of Ne-th triangle element:

           p2 
           |\
           | \ 
           |  \
          p0---p1
        #### **Example:**   
        >  0 1 2 3   
        >  1 2 3 4   

        for   

          2---3   
          |\  |  
          | \ |  
          |  \|  
          0---1  

    * Quadrilateral
        > **Format:** p0 p1 p2 p3   
        > Where [p0, p1, p2, p3] are global numbers of vertecies of Ne-th quadrilateral element:

          p2---p3 
          |    |
          |    |
          p0---p1
        #### **Example:**    
        >  0 1 2 3    

* #### Result file format
    * Scalar field
        > **Format:** q   
        > Where [q] is a float value for n-th vertex
        #### **Example:**    
        > 1   
        > 2   
        > 2  
        > 3 

    * Vector field
        > **Format:** ox oy qx qy  
        > Where [ox, oy] is vector origin and [qx, qy] are vector components

        #### **Example:**    
        > -1 0  
        > 2 2  
        > 2 -2  
        > 0 3 

**You must remember**, when you put *structural* files (elements and vertecies), consider selection of **mesh type**, and when you put your result file, consider selection of **field** <br>

![Meshtype](/RMrs/Meshtype.png "Mesh types")   

![Fieldtype](/RMrs/field.png "Field types")

## Color Pallete & Value Scale

![Palette](/RMrs/palette.png "View of the pallete")


* #### Scale
    Scale is preety simple:
    - lowest value - is a minimum value (minimum vector length)
    - highest value - is a maximum value (maximum vector length)  

    Colors go from lowest to highest value with a gradient, defined by color pallete.

* #### Palletes
    * Pallete file

        Pallete file is a JSON file (.spf extension), that satisfies following schema:

        ```Json
        > JSON SCHEMA
        {
            "$schema": "https://json-schema.org/draft/2020-12/schema",
            "title": "JSON Schema for scale palette",

            "type": "array",
            "items": {
               "properties": {
                  "ColorCode": {
                     "type": "string",
                     "pattern": "[\\da-fA-F]{6}"
                   },

                  "Weight": {
                     "type": "number",
                     "minimum": 0,
                     "maximum": 1
                   }
                },
               "required": [ "ColorCode", "Weight" ]
            }
        }
        ```
        Shortly, pallete file is a collection of objects with items:
        - ColorCode: a string of 6 hexa symbols, that represent color in RGB
        - Weight: a float from 0 to 1, that defines a position of a color on a unit section 

        Both required.
        
        So, output file will satisfy the schema, and input file must satisfy it too.

        Palette file can be saved or loaded in menu \"Files\"

        #### Example:
        ```Json
        > default.spf:
        [
        	{
        		"ColorCode" : "0000ff",
        		"Weight" : 0
        	},
        	{
        		"ColorCode" : "00ffff",
        		"Weight" : 0.333333333
        	},
        	{
        		"ColorCode" : "ffff00",
        		"Weight" : 0.666666666
        	},
        	{
        		"ColorCode" : "ff0000",
        		"Weight" : 1
        	},
        ]
        ```
    * Pallete redactor    
        ![Pallete](/RMrs/pallete%20color.png "Color redactor")  

        - Redactor consists of color redactor cells.
        R, G and B fields are constraint to a range from 0 to 255,
        Hex field is constriant to a range from 000000 to FFFFFF. 

        - Weight field and slider are constraint in a range from 0 to 1

        - Arrow button allows to swap weights with respective color node (🡩 swap with next on top node, 🡫 swap with next on top node)

        - X button removes a node


> **Note:** Pallete may be a bit laggy - when colors do not update try moving weight slider or change colors back and forth

## Controls
* Zooming with mouse wheel
* To move camera press LMB and drag

## Extra

### Vectors

![Vectors](/RMrs/vector.png "Vector settings")  
* #### Vector types (sets the visual type of arrows):
    * Thin lines   
![Vectors](/RMrs/vector_sol.png "Thin lines")  
    * Thick lines   
![Vectors](/RMrs/vector_sol2.png "Thick lines")   
    * Thin arrows   
![Vectors](/RMrs/vector_sol3.png "Thin arrows")   
    * Thick arrows   
![Vectors](/RMrs/vector_sol4.png "Thick arrows ")   
    
* #### Vector length:
    * *\"Consider length\" checkbox* - Defines if vector length is considered (if not, they're all equal to an average length)
    * *Slider for length* - Defines length multiplier [0.1 <-> 2]

### Menu
* #### Files    
    ![MenuFiles](/RMrs/menu_files.png "Menu \"files\" content")  

    * *\"Open vertex/element/result file\"* - Opens file selection window 
    * *\"Clear files\"* - Clears file selection, removes mesh
    * *\"Open scale pallete file\"* - Opens file selection window (opens pallete in json format)
    * *\"Save scale pallete file\"* - Opens file saving window (saves pallete in json format)
    

* #### Help   
    ![MenuHelp](/RMrs/menu_help.png "Menu \"help\" content")

    * [dev] *\"Recompile shaders\"* - Recompiles shaders, if they were changed
    * *\"Redraw\"* - Redraws mesh, if something that must change, did not change

### Other

![Other](/RMrs/other.png "Other")
* *\"Show grid\" checkbox* - Defines if grid should draw   
    ![Grid1](/RMrs/grid1.png)  
    ![Grid2](/RMrs/grid2.png)  
    ![Grid3](/RMrs/grid3.png)   
    ![Grid4](/RMrs/grid5.png)

* *\"Return to origin\" button* - Set camera center to (0, 0) and zoom to 1


