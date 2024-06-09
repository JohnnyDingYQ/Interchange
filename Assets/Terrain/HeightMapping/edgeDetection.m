img = imread("out.png", "png");
processed = edge(img);
% imshow(processed);
for i = 1:numel(processed)
    if processed(i) == 0
        processed(i) = 1;
    else
        processed(i) = 0;
    end
end
imwrite(processed, "edged.png", "png");