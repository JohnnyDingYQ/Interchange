img = imread("in.png", "png");
threshold = 2500;
clamp_high = 10000;
clamp_low = 0;
img = imresize(img, [4096, 4096], "bicubic");
for i = 1:numel(img)
    h = img(i);
    if h > threshold
        img(i) = clamp_high;
    else
        img(i) = clamp_low;
    end
end
% img = imbilatfilt(img);
% imshow(img);
imwrite(img, "out.png", "png");