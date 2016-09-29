%%
%addpath('MATLAB\libs\xdfimport1.13\');
% addpath('G:\Documents\MATLAB\libs\xdfimport1.13');
% 
% %% Load Markers from xdf file
% pathToRecording = 'G:\Recordings\ReactionTime\exp1\untitled.xdf';
% 
% xdf = load_xdf(pathToRecording);
% 
% marker = xdf{1};

%% Estimate Timings 

f = @(m) strcmp(m,'Begin Send Signal') == 1 || strcmp(m, 'End Send Signal') == 1;

arduinoComDuration = arrayfun(f, marker.time_series,'UniformOutput',1);

indices = find(arduinoComDuration);

ts = marker.time_stamps(indices);
mar = marker.time_series(indices);

%diff(ts)

lengthOfTs = length(ts);

timings = [];

for i = 2 : 2 : lengthOfTs

previous = ts(i-1);
current = ts(i);

timings(end+1) = current - previous;


end