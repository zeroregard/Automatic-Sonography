gv = 0:0.01:1;
[X,Y] = meshgrid(gv,gv);
ptCloud = loadpcd('pointcloud-2016-10-05_01-06-03-.pcd');
figure
pcshow(ptCloud);
title('Original Data');



noise = rand(500, 3);
ptCloudA = pointCloud([ptCloud.Location; noise]);

figure
pcshow(ptCloudA);
title('Noisy Data');
ptCloudB = pcdenoise(ptCloudA);

figure;
pcshow(ptCloudB);
title('Denoised Data');