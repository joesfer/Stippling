Description:

	This file relates to the 'Stippling and Blue Noise' article, published on http://www.joesfer.com/?p=108

License:

	This software is released under the LGPL-3.0 license: http://www.opensource.org/licenses/lgpl-3.0.html	

	Copyright (c) 2012, Jose Esteve. http://www.joesfer.com
	
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 3.0 of the License, or (at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

Contents of the package:

	/bin
		BlueNoise.exe : compiled version of the sample application
		wangTileSet_1024spt_3cols.xml : serialized set of wang tiles with 1024 
						samples per tile, 3 edge colors, ready 
						to be used as source on the Wang Tiles 
stippling method.

	/source
		C# project with the source code for the test application, including 
		implementations of the Adaptive Incremental Sampling, Recursive Wang Tiles, 
		Voronoi Diagram and Delaunay Triangulation.