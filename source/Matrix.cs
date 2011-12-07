/* 
	================================================================================
	Copyright (c) 2011, Jose Esteve. http://www.joesfer.com
	All rights reserved. 

	Redistribution and use in source and binary forms, with or without modification, 
	are permitted provided that the following conditions are met: 

	* Redistributions of source code must retain the above copyright notice, this 
	  list of conditions and the following disclaimer. 
	
	* Redistributions in binary form must reproduce the above copyright notice, 
	  this list of conditions and the following disclaimer in the documentation 
	  and/or other materials provided with the distribution. 
	
	* Neither the name of the organization nor the names of its contributors may 
	  be used to endorse or promote products derived from this software without 
	  specific prior written permission. 

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR 
	ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON 
	ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
	================================================================================
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueNoise
{
    class Matrix3
    {
        public Matrix3(float m00, float m01, float m02,
                float m10, float m11, float m12,
                float m20, float m21, float m22)
        {
            m = new float[3, 3];
            m[0,0] = m00; m[0,1] = m01; m[0,2] = m02;
            m[1,0] = m10; m[1,1] = m11; m[1,2] = m12;
            m[2,0] = m20; m[2,1] = m21; m[2,2] = m22;
        }
        public float Determinant()
        {
            float det2_12_01 = m[1,0] * m[2,1] - m[1,1] * m[2,0];
            float det2_12_02 = m[1,0] * m[2,2] - m[1,2] * m[2,0];
            float det2_12_12 = m[1,1] * m[2,2] - m[1,2] * m[2,1];

            return m[0,0] * det2_12_12 - m[0,1] * det2_12_02 + m[0,2] * det2_12_01;
        }
        public float[,] m;
    }

    class Matrix4
    {
        public Matrix4(float m00, float m01, float m02, float m03,
                       float m10, float m11, float m12, float m13,
                       float m20, float m21, float m22, float m23,
                       float m30, float m31, float m32, float m33)
        {
            m = new float[4, 4];
            m[0, 0] = m00; m[0, 1] = m01; m[0, 2] = m02; m[0, 3] = m03;
            m[1, 0] = m10; m[1, 1] = m11; m[1, 2] = m12; m[1, 3] = m13;
            m[2, 0] = m20; m[2, 1] = m21; m[2, 2] = m22; m[2, 3] = m23;
            m[3, 0] = m30; m[3, 1] = m31; m[3, 2] = m32; m[3, 3] = m33;
        }
        public float Determinant()
        {
            // 2x2 sub-determinants
            float det2_01_01 = m[0,0] * m[1,1] - m[0,1] * m[1,0];
            float det2_01_02 = m[0,0] * m[1,2] - m[0,2] * m[1,0];
            float det2_01_03 = m[0,0] * m[1,3] - m[0,3] * m[1,0];
            float det2_01_12 = m[0,1] * m[1,2] - m[0,2] * m[1,1];
            float det2_01_13 = m[0,1] * m[1,3] - m[0,3] * m[1,1];
            float det2_01_23 = m[0,2] * m[1,3] - m[0,3] * m[1,2];

            // 3x3 sub-determinants
            float det3_201_012 = m[2,0] * det2_01_12 - m[2,1] * det2_01_02 + m[2,2] * det2_01_01;
            float det3_201_013 = m[2,0] * det2_01_13 - m[2,1] * det2_01_03 + m[2,3] * det2_01_01;
            float det3_201_023 = m[2,0] * det2_01_23 - m[2,2] * det2_01_03 + m[2,3] * det2_01_02;
            float det3_201_123 = m[2,1] * det2_01_23 - m[2,2] * det2_01_13 + m[2,3] * det2_01_12;

            return (-det3_201_123 * m[3,0] + det3_201_023 * m[3,1] - det3_201_013 * m[3,2] + det3_201_012 * m[3,3]);
        }
        public float[,] m;
    }
}
