
#include <stdio.h>
#include <iostream>
#include <pcl\io\pcd_io.h>
#include <pcl\common\common.h>
//http://stackoverflow.com/questions/16514762/point-cloud-library-on-visual-studio

using namespace std;
using namespace pcl;
using namespace io;

int main(int argc, char** argv)
{
	PointCloud<PointXYZ>::Ptr cloud(new PointCloud <PointXYZ>);
	
	if (loadPCDFile<PointXYZ>("test_pcd.pcd", *cloud) == -1) //* load the file
	{
		PCL_ERROR("Couldn't read file test_pcd.pcd \n");
		return (-1);
	}
	std::cout << "Loaded "
		<< cloud->width * cloud->height
		<< " data points from test_pcd.pcd with the following fields: "
		<< std::endl;
	for (size_t i = 0; i < cloud->points.size(); ++i)
		std::cout << "    " << cloud->points[i].x
		<< " " << cloud->points[i].y
		<< " " << cloud->points[i].z << std::endl;
	return (0);
}

