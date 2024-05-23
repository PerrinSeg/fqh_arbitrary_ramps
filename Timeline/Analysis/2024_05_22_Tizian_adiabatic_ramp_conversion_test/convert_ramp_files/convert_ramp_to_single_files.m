% 2024/05/07: Convert all ramps from .txt file to a new file, 
% keeping a separate file for each exp channel

clear
close all


%% Load ramp file

ramp = readmatrix('ramp_points.txt'); 
% times, J_x, J_y, Delta_x, Delta_y
ramp_time_full = ramp(:, 1); 
% ramp_amps = ramp(:, 2:end); 

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
mid_idx = 5;
ramp_gauge = 1;
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
ramp_time = ramp(mid_idx:end, 1) - ramp(mid_idx);
ramp_Jx = ramp(mid_idx:end, 2);
ramp_Jy = ramp(mid_idx:end, 3);
ramp_Dx = ramp(mid_idx:end, 4);
ramp_Dy = ramp(mid_idx:end, 5);

% Constants
h = 6.6260695729 * 10^(-34);
hbar = h / 2 / pi;
E_r = 1.24 * 10^3;      % E_r in Hz

% tau = 4.3*10^-3;
% J0 = hbar/tau/h;
J0 = 40.5;
tau = 1/(2*pi*J0);

ramp_time_ms_aux = ramp_time*tau*10^3;
ramp_Jx_hz = ramp_Jx*J0;
ramp_Jy_hz = ramp_Jy*J0;
ramp_Dx_hz = ramp_Dx*J0;
ramp_Dy_hz = ramp_Dy*J0;


%% Plot raw values

%%%%%%%%%%%%%%%%
plot_figure = 1;
save_figure = 1;
%%%%%%%%%%%%%%%%
if plot_figure
    clear ax
    figure
    tl = tiledlayout('flow', "TileSpacing",'compact', 'Padding','compact');
    
    ax(1) = nexttile;
    hold on
    plot(ramp_time, ramp_Jx, 'o-', 'DisplayName', 'J_{quad}')
    plot(ramp_time, ramp_Jy, 'o-', 'DisplayName', 'J_{quic}')
    ylabel('J (J)')
    legend('location','best')

    ax(2) = nexttile;
    hold on
    plot(ramp_time, ramp_Dx, 'o-', 'DisplayName', '\Delta_{quad}')
    plot(ramp_time, ramp_Dy, 'o-', 'DisplayName', '\Delta_{quic}')
    ylabel('\Delta (J)')
    legend('location','best')

    xlabel(tl,'time (\tau)')
    linkaxes(ax,'x')
    if save_figure
        print('ramp_initial','-dpng')
    end
end


%% Interpolate ramps with correct number of points

N_pts_new  = 10000; % points needed for NI function
ramp_times_new = linspace(0,1,N_pts_new);
ramp_time_norm = ramp_time/ramp_time(end);

ramp_Jx_new = interp1(ramp_time_norm, ramp_Jx, ramp_times_new, 'linear');
ramp_Jy_new = interp1(ramp_time_norm, ramp_Jy, ramp_times_new, 'linear');
ramp_Dx_new = interp1(ramp_time_norm, ramp_Dx, ramp_times_new, 'linear');
ramp_Dy_new = interp1(ramp_time_norm, ramp_Dy, ramp_times_new, 'linear');

%%%%%%%%%%%%%%%%
plot_figure = 0;
save_figure = 0;
%%%%%%%%%%%%%%%%
if plot_figure
    clear ax
    figure()
    tiledlayout('Flow', TileSpacing = 'compact', Padding = 'compact')
    
    ax(1) = nexttile;
    hold on
    plot(ramp_times_new, ramp_Jx_new, '-', 'LineWidth', 1.5, 'DisplayName', 'J_{quad}', 'SeriesIndex', 1)
    plot(ramp_time_norm, ramp_Jx, 'o', 'LineWidth', 1.5, 'DisplayName', 'J_{quad}', 'SeriesIndex', 1)

    plot(ramp_times_new, ramp_Jy_new, '-', 'LineWidth', 1.5, 'DisplayName', 'J_{quic}', 'SeriesIndex', 2)
    plot(ramp_time_norm, ramp_Jy, 'o', 'LineWidth', 1.5, 'DisplayName', 'J_{quic}', 'SeriesIndex', 2)
    ylabel('J (J)')
    legend(Location = 'best')

    ax(2) = nexttile;
    hold on
    plot(ramp_times_new, ramp_Dx_new, '-', 'LineWidth', 1.5, 'DisplayName', '\Delta_{quad}', 'SeriesIndex', 1)
    plot(ramp_time_norm, ramp_Dx, 'o', 'LineWidth', 1.5, 'DisplayName', '\Delta_{quad}', 'SeriesIndex', 1)

    plot(ramp_times_new, ramp_Dy_new, '-', 'LineWidth', 1.5, 'DisplayName', '\Delta_{quic}', 'SeriesIndex', 2)
    plot(ramp_time_norm, ramp_Dy, 'o', 'LineWidth', 1.5, 'DisplayName', '\Delta_{quic}', 'SeriesIndex', 2)
    ylabel('\Delta (J)')
    legend(Location = 'best')
    
    xlabel('Time (arb.)')
    linkaxes(ax, 'x')

    if save_figure
        print('J_ramp_interpolated', '-dpng')
    end
end


%% Convert interpolated Tilt ramps to voltage

% TO DO: Do reverse-fit here too to capture non-linearity at small tilt values??

% Calibration parameters
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% quad
sx = 1.61310 * 10^3; % 2024/01/17
offx = -0.0633*10^3;

% quic
sy = 0.1333*10^3; % 2024/05/03
offy = -0.1673*10^3;
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
tiltToVolt = @(Delta, off, s) (Delta*J0 - off)/s;
voltToTilt = @(v, off, s) (v*s + off)/J0;

ramp_DVx = tiltToVolt(ramp_Dx_new, offx, sx);
ramp_DVy = tiltToVolt(ramp_Dy_new, offy, sy);


%% Convert interpolated J_quad ramp to voltage

if ramp_gauge
    % Calibrated values
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    b1 = 108.4;                 % 1st fit coeff from V to J calibration         
    b2 = 1.399 * 10^-6;         % 2nd fit coeff from V to J calibration
    dcal_quad = 7.4*10^3/E_r;   % lattice depth vcal_quad
    vcal_quad = 3.34;           % lattice depth calibration voltage
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    b2_full = b2*10^(2*vcal_quad)/dcal_quad;
    
    % Full V -> J equation from exp calibraton
    voltToJx = @(v)  b1/J0 * besselj( 1, b2*10.^(2*v) );
    
    % Break into two steps to make numerics easier (i.e. get rid of divergence near J = 0)
    voltToDepthx = @(v) dcal_quad * 10.^(2*(v-vcal_quad));    % Step 1: convert voltage to lattice depth (in E_r)
    depthToJx = @(d)  b1/J0 * besselj( 1, b2_full*d );           % Step 2: convert lattice depth to J
    depthToVoltx = @(d) (1/2)*log10(d/dcal_quad) + vcal_quad; % Step 1 can be inverted analytically
    
    % Numerically invert depth -> J equation to get best value for each point
    
    % convert Jx (J) -> Raman power d_R (E_r)
    tic
    d_r = zeros(1,length(ramp_Jx_new));
    syms d
    parfor i = 1:length(ramp_Jx_new)
        jx = ramp_Jx_new(i);
        if jx == 0
            val = 0;
        else
            val = vpasolve( jx == depthToJx(d), d, [0, 1.5] );
        end
       d_r(i) = val;
    end
    toc
else
    % Calibrated values
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    cx = 1068*10^(-3);            % 1st fit coeff from V to J calibration (2024/03/21)
    lattice1_calib_volt = 3.76; %V
    lattice1_calib_depth = 30.3; % E_r
    lattice1_voltage_offset = -0.14;
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    depthToVoltx = @(d) lattice1_voltage_offset + lattice1_calib_volt + 1/2 * log10(d/lattice1_calib_depth); % depth in E_r
    depthToJx = @(d) cx * 4/sqrt(pi) * E_r/J0 * d.^(3/4) .* exp(-2 * d.^(1/2)); % depth (E_r) --> J_x (J)
    voltToDepthx = @(v) lattice1_calib_depth * 10.^(2*(v - lattice1_calib_volt - lattice1_voltage_offset));
    voltToJx = @(v) depthToJx(voltToDepthx(v));

    % Numerically invert depth -> J equation to get best value for each point
    % convert Jy (J) -> 2D2 lattice depth (E_r)
    tic
    d_r = zeros(1,length(ramp_Jx_new));
    syms d
    parfor i = 1:length(ramp_Jx_new)
        jx = ramp_Jx_new(i);
        if jx > depthToJx(45)
            val = vpasolve( jx == depthToJx(d), d, [1, 45] );
        else
            val = 45;
        end
        d_r(i) = val;
    end
    toc
end

% convert d_R (E_r) -> input voltage (V)
ramp_KVx = depthToVoltx(d_r);
ramp_KVx(ramp_KVx < 0) = 0;

% %% Check results
% 
% Dx_aux = linspace(1, 10, 1000);
% Jx_aux = depthToJx(Dx_aux);
% 
% % Plot
% %%%%%%%%%%%%%%%%%
% plot_figure = 1;
% %%%%%%%%%%%%%%%%%
% if plot_figure
%     figure
%     tl2 = tiledlayout('flow','tilespacing','compact');
% 
%     ax1 = nexttile;
%     hold on
%     plot(ramp_Jx_new, d_r, '-', 'DisplayName', 'numeric')
%     plot(Jx_aux, Dx_aux, '--', 'DisplayName', 'full')
%     ylabel('2D1 depth (E_r)')
%     legend('location','best')
% 
%     xlabel(tl2, 'J_x (J)')
%     title(tl2, 'Voltage check quic')
% end


%% Check results

Vx_aux = depthToVoltx(linspace(1, 10, 10000));
Jx_aux = voltToJx(Vx_aux);

% Plot
%%%%%%%%%%%%%%%%%
plot_figure = 1;
%%%%%%%%%%%%%%%%%
if plot_figure
    figure
    tl2 = tiledlayout('flow','tilespacing','compact');
    
    ax1 = nexttile;
    hold on
    plot(ramp_Jx_new, ramp_KVx, '-', 'DisplayName', 'numeric')
    plot(Jx_aux, Vx_aux, '--', 'DisplayName', 'full')
    ylabel('Voltage (V)')
    legend('location','best')

    xlabel(tl2, 'J_x (J)')
    title(tl2, 'lattiVoltage check quad')
    % linkaxes([ax1,ax2],'x')
    xlim([0 1.3])
end


%% Convert interpolated J_quic ramp to 2D2 lattice depth (in V)

% Calibrated values
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
c = 991*10^(-3);            % 1st fit coeff from V to J calibration (2024/04/08)
lattice2_calib_volt = 3.56;
lattice2_calib_depth = 51.5;
lattice2_voltage_offset = -0.11;
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
depthToVolty = @(x) lattice2_voltage_offset + lattice2_calib_volt + 1/2 * log10(x/lattice2_calib_depth);
depthToJy = @(d) c * 4/sqrt(pi) * E_r/J0 * d.^(3/4) .* exp(-2 * d.^(1/2));
voltToDepthy = @(v) lattice2_calib_depth * 10.^(2*(v - lattice2_voltage_offset - lattice2_calib_volt));
voltToJy = @(v) c * 4/sqrt(pi) * E_r/J0 * (voltToDepthy(v)).^(3/4) .* exp(-2 * (voltToDepthy(v)).^(1/2)); 

% Numerically invert depth -> J equation to get best value for each point
% convert Jy (J) -> 2D2 lattice depth (E_r)
tic
d_r = zeros(1, length(ramp_Jy_new));
syms d
parfor i = 1:length(ramp_Jy_new)
    jy = ramp_Jy_new(i);
    if jy > depthToJy(45)
        val = vpasolve( jy == depthToJy(d), d, [1, 45] );        
    else
        val = 45;
    end
    d_r(i) = val;
end
toc

% convert d_R (E_r) -> input voltage (V)
ramp_JVy = depthToVolty(d_r);
ramp_JVy(ramp_JVy <= 0) = 0;

% %% Check results
% 
% Dy_aux = linspace(4, 10, 1000);
% Jy_aux = depthToJy(Dy_aux);
% 
% % Plot
% %%%%%%%%%%%%%%%%%
% plot_figure = 1;
% %%%%%%%%%%%%%%%%%
% if plot_figure
%     figure
%     tl2 = tiledlayout('flow','tilespacing','compact');
% 
%     ax1 = nexttile;
%     hold on
%     plot(ramp_Jy_new, d_r, '-', 'DisplayName', 'numeric')
%     plot(Jy_aux, Dy_aux, '--', 'DisplayName', 'full')
%     ylabel('2D2 depth (E_r)')
%     legend('location','best')
% 
%     xlabel(tl2, 'J_y (J)')
%     title(tl2, 'Voltage check quic')
% end


%% Check results

Vy_aux = depthToVolty(linspace(4, 10, 10000));
Jy_aux = voltToJy(Vy_aux);

% Plot
%%%%%%%%%%%%%%%%%
plot_figure = 1;
%%%%%%%%%%%%%%%%%
if plot_figure
    figure
    tl2 = tiledlayout('flow','Tilespacing','compact');
    
    ax1 = nexttile;
    hold on
    plot(ramp_Jy_new, ramp_JVy, '-', 'DisplayName', 'numeric')
    plot(Jy_aux, Vy_aux, '--', 'DisplayName', 'full')
    ylabel('2D2 voltage (V)')
    legend('location','best')

    xlabel(tl2, 'J_y (J)')
    title(tl2, 'Voltage check quic')
end


%% Plot results as a function of time

% Normalize V to lie between 0 and 1 (TO DO: is this necessary??? double check...)
% ramp_KVx_new = (ramp_KVx - min(ramp_KVx)) / (max(ramp_KVx) - min(ramp_KVx));
% if min(ramp_KVx) == max(ramp_KVx) 
%     ramp_KVx_new = ones(size(ramp_KVx));
% end
% ramp_KVx_comb = [ramp_times_new', ramp_KVx_new'];
% ramp_KVx_comb = [ramp_times_new', ramp_KVx'];
ramp_start = ramp_KVx(1);
ramp_end = ramp_KVx(end);
if ramp_start >= ramp_end
    ramp_KVx_new = (ramp_KVx - ramp_end) / (ramp_start - ramp_end);
else
    ramp_KVx_new = flip((ramp_KVx - ramp_start) / (ramp_end - ramp_start));
end
if min(ramp_KVx) == max(ramp_KVx) 
    ramp_KVx_new = ones(size(ramp_KVx));
end
ramp_KVx_comb = [ramp_times_new', ramp_KVx_new'];

% ramp_JVy_new = (ramp_JVy - min(ramp_JVy)) / (max(ramp_JVy) - min(ramp_JVy));
% if min(ramp_JVy) == max(ramp_JVy) 
%     ramp_KVy_new = ones(size(ramp_JVy));
% end
% ramp_JVy_comb = [ramp_times_new', ramp_JVy_new'];
% ramp_JVy_comb = [ramp_times_new', ramp_JVy'];
ramp_start = ramp_JVy(1);
ramp_end = ramp_JVy(end);
if ramp_start >= ramp_end
    ramp_JVy_new = (ramp_JVy - ramp_end) / (ramp_start - ramp_end);
else
    ramp_JVy_new = flip((ramp_JVy - ramp_start) / (ramp_end - ramp_start));
end
if min(ramp_JVy) == max(ramp_JVy) 
    ramp_JVy_new = ones(size(ramp_JVy));
end
ramp_JVy_comb = [ramp_times_new', ramp_JVy_new'];

% ramp_DVx_new = (ramp_DVx - min(ramp_DVx)) / (max(ramp_DVx) - min(ramp_DVx));
% if min(ramp_DVx) == max(ramp_DVx) 
%     ramp_DVx_new = ones(size(ramp_DVx));
% end
% ramp_DVx_comb = [ramp_times_new', ramp_DVx_new'];
% ramp_DVx_comb = [ramp_times_new', ramp_DVx'];
ramp_start = ramp_DVx(1);
ramp_end = ramp_DVx(end);
if ramp_start >= ramp_end
    ramp_DVx_new = (ramp_DVx - ramp_end) / (ramp_start - ramp_end);
else
    ramp_DVx_new = flip((ramp_DVx - ramp_start) / (ramp_end - ramp_start));
end
if min(ramp_DVx) == max(ramp_DVx) 
    ramp_DVx_new = ones(size(ramp_DVx));
end
ramp_DVx_comb = [ramp_times_new', ramp_DVx_new'];

% ramp_DVy_new = (ramp_DVy - min(ramp_DVy)) / (max(ramp_DVy) - min(ramp_DVy));
% if min(ramp_DVy) == max(ramp_DVy) 
%     ramp_DVy_new = ones(size(ramp_DVy));
% end
% ramp_DVy_comb = [ramp_times_new', ramp_DVy_new'];
% ramp_DVy_comb = [ramp_times_new', ramp_DVy'];
ramp_start = ramp_DVy(1);
ramp_end = ramp_DVy(end);
if ramp_start >= ramp_end
    ramp_DVy_new = (ramp_DVy - ramp_end) / (ramp_start - ramp_end);
else
    ramp_DVy_new = flip((ramp_DVy - ramp_start) / (ramp_end - ramp_start));
end
if min(ramp_DVy) == max(ramp_DVy) 
    ramp_DVy_new = ones(size(ramp_DVy));
end
ramp_DVy_comb = [ramp_times_new', ramp_DVy_new'];

% Plot
%%%%%%%%%%%%%%%%%
plot_figure = 1;
save_figure = 0;
%%%%%%%%%%%%%%%%%
if plot_figure
    clear ax
    figure('Units','normalized', 'OuterPosition', [0.3, 0.05, 0.3, 0.95])
    t = tiledlayout(4,1,'Tilespacing','compact', 'Padding','compact');
    
    ax(1) = nexttile;
    plot(ramp_KVx_comb(:,1), ramp_KVx, '-', DisplayName = 'numeric', LineWidth=1.5)
    ylabel('Gauge voltage (V)')

    ax(2) = nexttile;
    plot(ramp_JVy_comb(:,1), ramp_JVy, '-', DisplayName = 'numeric', LineWidth=1.5)
    ylabel('2D2 lattice voltage (V)')

    ax(3) = nexttile;
    plot(ramp_DVx_comb(:,1), ramp_DVx, '-', DisplayName = 'numeric', LineWidth=1.5)
    ylabel('\Delta_{quad} PS8 voltage (V)')

    ax(4) = nexttile;
    plot(ramp_DVy_comb(:,1), ramp_DVy, '-', DisplayName = 'numeric', LineWidth=1.5)
    ylabel('\Delta_{quic} PS5 voltage (V)')

    xlabel(t, 'time (\tau)')
    title(t, 'Final voltage ramp')
    linkaxes(ax, 'x')

    if save_figure
        print('Ramp_final', '-dpng')
    end
end

% Plot
%%%%%%%%%%%%%%%%%
plot_figure = 1;
save_figure = 1;
%%%%%%%%%%%%%%%%%
if plot_figure
    clear ax
    figure('Units','normalized', 'OuterPosition', [0.3, 0.05, 0.3, 0.95])
    t = tiledlayout(4,1,'Tilespacing','compact', 'Padding','compact');

    ax(1) = nexttile;
    plot(ramp_KVx_comb(:,1), ramp_KVx_comb(:,2), '-', DisplayName = 'numeric', LineWidth=1.5)
    ylabel('Gauge voltage (V)')

    ax(2) = nexttile;
    plot(ramp_JVy_comb(:,1), ramp_JVy_comb(:,2), '-', DisplayName = 'numeric', LineWidth=1.5)
    ylabel('2D2 lattice voltage (V)')

    ax(3) = nexttile;
    plot(ramp_DVx_comb(:,1), ramp_DVx_comb(:,2), '-', DisplayName = 'numeric', LineWidth=1.5)
    ylabel('\Delta_{quad} PS8 voltage (V)')

    ax(4) = nexttile;
    plot(ramp_DVy_comb(:,1), ramp_DVy_comb(:,2), '-', DisplayName = 'numeric', LineWidth=1.5)
    ylabel('\Delta_{quic} PS5 voltage (V)')

    xlabel(t, 'time (\tau)')
    title(t, 'Final voltage ramp')
    linkaxes(ax, 'x')

    if save_figure
        print('Ramp_final', '-dpng')
    end
end


%% Check by converting back to J and Delta, and comparing with original ramp

% Plot
%%%%%%%%%%%%%%%%%
plot_figure = 1;
save_figure = 1;
%%%%%%%%%%%%%%%%%
if plot_figure
    figure('Units','normalized', 'OuterPosition', [0.3, 0.05, 0.3, 0.95])
    t = tiledlayout(4,1,'Tilespacing','compact','Padding','compact');
    
    nexttile
    hold on
    plot(ramp_KVx_comb(:,1), voltToJx(ramp_KVx), '-', DisplayName = 'converted', LineWidth = 1.5)
    plot(ramp_time/ramp_time(end), ramp_Jx, '.', MarkerSize = 15, DisplayName = 'K_{quad}')
    ylabel('K_{quad} (J)')

    nexttile
    hold on
    plot(ramp_JVy_comb(:,1), voltToJy(ramp_JVy), '-', DisplayName = 'converted', LineWidth = 1.5)
    plot(ramp_time/ramp_time(end), ramp_Jy, '.', MarkerSize = 15, DisplayName = 'J_{quic}')
    ylabel('J_{quic} (J)')

    nexttile
    hold on
    plot(ramp_DVx_comb(:,1), voltToTilt(ramp_DVx, offx, sx), '-', DisplayName = 'converted', LineWidth = 1.5)
    plot(ramp_time/ramp_time(end), ramp_Dx, '.', MarkerSize = 15, DisplayName = '\Delta_{quad}')
    ylabel('\Delta_{quad} (J)')

    nexttile
    hold on
    plot(ramp_DVy_comb(:,1), voltToTilt(ramp_DVy, offy, sy), '-', DisplayName = 'converted', LineWidth = 1.5)
    plot(ramp_time/ramp_time(end), ramp_Dy, '.', MarkerSize = 15, DisplayName = '\Delta_{quad}')
    ylabel('\Delta_{quic} (J)')


    xlabel(t, 'time (\tau)')
    title(t, 'Final ramp')

    if save_figure
        print('Ramp_check', '-dpng')
    end
end
 

%% Save new interpolated ramp as .txt file

%%%%%%%%%%%%%%%
save_ramp = 1;
%%%%%%%%%%%%%%%
% save as txt file 
if save_ramp

    if ramp_gauge 
        fid = fopen('GaugeVx_ramp.txt', 'w');
        fprintf(fid, '%f %f\r\n', ramp_KVx_comb');
        fclose(fid);
    else
        fid = fopen('KVx_ramp.txt', 'w');
        fprintf(fid, '%f %f\r\n', ramp_KVx_comb');
        fclose(fid);
    end

    fid = fopen('JVy_ramp.txt', 'w');
    fprintf(fid, '%f %f\r\n', ramp_JVy_comb');
    fclose(fid);

    fid = fopen('DVx_ramp.txt', 'w');
    fprintf(fid, '%f %f\r\n', ramp_DVx_comb');
    fclose(fid);

    fid = fopen('DVy_ramp.txt', 'w');
    fprintf(fid, '%f %f\r\n', ramp_DVy_comb');
    fclose(fid);
end

