clear 
close all

%% read ramp file

ramp = readmatrix('ramp_points.txt'); 
ramp_time = ramp(:, 1); 
ramp_amps = ramp(:, 2:end); 
% ramp_amps(:,1) = J_x
% ramp_amps(:,2) = J_y
% ramp_amps(:,3) = Delta_x
% ramp_amps(:,4) = Delta_y


%% Constants

tau = 4.3*10^-3;
h = 6.6260695729 * 10^(-34);
hbar = h / 2 / pi;
J0 = hbar/tau/h;
ntimes = length(ramp_time);
nvariables = 6;

ramp_time_ms = ramp_time*tau*10^3;
ramp_amps_hz = ramp_amps*J0;


%% Plot raw values

plot_figure = 1;
save_figure = 0;
if plot_figure
    figure
    tl = tiledlayout(2,1,"TileSpacing",'compact','Padding','compact');
    
    ax1 = nexttile;
    plot(ramp_time*tau*10^3, ramp_amps(:,1)*J0, '.-', 'DisplayName','J_{quad}')
    hold on
    plot(ramp_time*tau*10^3, ramp_amps(:,2)*J0, '.-', 'DisplayName','J_{quic}')
    ylabel('J (Hz)')
    legend('location','best')

    ax2 = nexttile;
    plot(ramp_time*tau*10^3, ramp_amps(:,3)*J0, '.-', 'DisplayName','\Delta_{quad}')
    hold on
    plot(ramp_time*tau*10^3, ramp_amps(:,4)*J0, '.-', 'DisplayName','\Delta_{quic}')
    ylabel('\Delta (Hz)')
    legend('location','best')

    xlabel(tl,'time (ms)')
    linkaxes([ax1,ax2],'x')
    if save_figure
        print('ramp_arb_initial_channels','-dpng')
    end
end


%% apply appropriate conversions to parameters

ramp_amps_convert = zeros(ntimes, nvariables);
% convert trom tunneling along quic to lattice depth in E_r
% convert trom tunneling along quad to Gauge beam voltage
% convert from quic tilt to DAC voltage (ps5)
% convert from quad tilt to either ps8 DAC voltage OR gauge beam freq (voltage)


%% quad lattice depth 

quad_latt_depth_init = 45;
quad_latt_depth_final = 5;
quad_latt_ramp_start_idx = 6;
quad_latt_ramp_end_idx = 7;

quadDepth = zeros(ntimes,1);
quadDepth = quadDepth + quad_latt_depth_init;
quadDepth(quad_latt_ramp_end_idx:end) = quad_latt_depth_final; % TO DO: adjust so that if it spans multiple points, it linearly interpolates between start and end depths
% quadDepth
ramp_amps_convert(:,1) = quadDepth;


%% quad Gauge beam power (assuming 2D1 lattice depth 5Er)

quadtunneling = ramp_amps(:,1)*J0;
quadvolts = zeros(size(quadtunneling));
syms v
for i = 1:length(quadvolts)
    jx = quadtunneling(i);
    if jx == 0
        val = 0;
    else
        val = vpasolve( jx == 108.4 * besselj(1, 1.399 * 10^(2*v-6)), v, [0, 3.05] );
    end
    quadvolts(i) = val;
end
% ramp_amps_convert(:,2) = quadvolts;
ramp_amps_convert(:,2) = quadtunneling;


%% quic depth

quictunneling = ramp_amps(:,2)*J0;
quicdepth = zeros(size(quictunneling));
syms x
for i = 1:length(quictunneling)
    jy = quictunneling(i);
    if jy == 0
        val = 45;
    else   
        val = vpasolve(jy == 1229*4/sqrt(pi)*124/100*x^(3/4)*exp(-2*x^(1/2)), x, [1,Inf]);
    end
    quicdepth(i) = val;
end
% ramp_amps_convert(:,3) = quicdepth;
ramp_amps_convert(:,3) = quictunneling;


%% Gauge beam freq

gauge_freq_hz = 700;

gauge_freq_full = zeros(ntimes,1);
gauge_freq_full = gauge_freq_full + gauge_freq_hz;
ramp_amps_convert(:,4) = gauge_freq_full;


%% quad magnetic gradient

quad_grad_ramp_start_idx = 4;
quad_grad_ramp_end_idx = 5;
quad_grad_hold_end_idx = quad_latt_ramp_end_idx;

high_grad_level_Hz = 950;
low_grad_level_Hz = 0;

high_grad_level_V = (high_grad_level_Hz + 63.3)/1631.0;
low_grad_level_V = (low_grad_level_Hz + 63.3)/1631.0;

quad_ramp_full = (ramp_amps(:,3)*J0 + gauge_freq_full + 63.3)/1631.0; %2024/01/17

quad_ramp_full(1:quad_grad_ramp_start_idx) = low_grad_level_V;
quad_ramp_full(quad_grad_ramp_end_idx:quad_grad_hold_end_idx) = high_grad_level_V; 

ramp_amps_convert(:,5) = quad_ramp_full;


%% quic magnetic gradient

ramp_amps_convert(:,6) = (ramp_amps(:,4)*J0 + 290.3)/134.6; % 2024/01/09


%% convert time points to durations

% convert from tunneling times to ms

% ramp_time = ramp_time*tau*10^3;
ramp_duration = [0; ramp_time_ms(2:end)-ramp_time_ms(1:end-1)];


%% save as new file

ramp_new = [ramp_duration, ramp_amps_convert];
ramp_new
% save as txt file 
fid = fopen('ramp_segments.txt','w');
fprintf(fid, '%f %f %f %f %f %f %f\r\n', ramp_new');
fclose(fid);

% ramp_new(:,0) = Delta t
% ramp_new(:,1) = V0_x
% ramp_new(:,2) = V_gauge (voltage corresponding to gauge power)
% ramp_new(:,3) = V0_y
% ramp_new(:,4) = gauge detuning V
% ramp_new(:,5) = gradV_x
% ramp_new(:,6) = gradV_y


%% Plot final values

plot_figure = 1;
save_figure = 0;
if plot_figure
    figure
    tl2 = tiledlayout(5,1,"TileSpacing",'compact','Padding','compact');
    
    ax1 = nexttile;
    plot(ramp_time_ms, ramp_amps_convert(:,1), '.-', 'DisplayName', 'quad')
    hold on
    plot(ramp_time_ms, ramp_amps_convert(:,3), '.-', 'DisplayName', 'quic')
    ylabel('Lattice depth (E_r)')
    legend('location','best')

    ax2 = nexttile;
    plot(ramp_time_ms, ramp_amps_convert(:,2), '.-', 'DisplayName', 'gauge power')
    ylabel('gauge power (V)')

    ax3 = nexttile;
    plot(ramp_time_ms, ramp_amps_convert(:,4), '.-', 'DisplayName', 'gauge freq')
    ylabel('gauge freq (Hz)')

    ax4 = nexttile;
    plot(ramp_time_ms, ramp_amps_convert(:,5), '.-', 'DisplayName','quad')
    ylabel('Quad magnetic gradient (V)')

    ax5 = nexttile;
    plot(ramp_time_ms, ramp_amps_convert(:,6), '.-', 'DisplayName','quic')
    ylabel('Quic magnetic gradient (V)')

    xlabel(tl2,'time (ms)')
    linkaxes([ax1,ax2,ax3,ax4,ax5],'x')

    if save_figure
        print('ramp_arb_final_channels','-dpng')
    end
end

%% get interpolated ramps
% rampt_full = zeros(ntimes, 100)
ramp_amps_full = [];
rampt_full = [];
dt = 1;
for i = 1:ntimes-1
    rampt_interp = ramp_time_ms(i):dt:ramp_time_ms(i+1); 
    ramp_amps_slopes = (ramp_amps_convert(i+1,:)-ramp_amps_convert(i,:))./(ramp_time_ms(i+1)-ramp_time_ms(i));
    ramp_amps_interp = zeros(numel(rampt_interp), numel(ramp_amps_slopes));
    for j = 1:numel(ramp_amps_slopes)
        ramp_amps_interp(:,j) = ramp_amps_slopes(j) .* (rampt_interp-rampt_interp(1)) + ramp_amps_convert(i,j);
    end
    ramp_amps_full = [ramp_amps_full; ramp_amps_interp];
    rampt_full = [rampt_full, rampt_interp];
end

% figure
% plot(rampt_full, ramp_amps_full(:,6),'DisplayName', 'Interpolated')
% hold on
% plot(ramp_time_ms, ramp_amps_convert(:,6),'DisplayName', 'Raw')
% legend('Location','best')



%% Interpolate quic depth 

quiclim = 60;
norder = 10;

quicDepthCal = 1.229;
Er = 1240;
a1 = 9/16;
a2 = Er*quicDepthCal;
quicJ_aux = linspace(0,quiclim,200);

% comparing quic analytic to numeric calc
jyToDepthFun = @(j0) a1*(lambertw(-1,-(2*j0./a2).^(2/3)/3 * pi^(1/3))).^2;
quicdepth_er_2 = real(jyToDepthFun(quicJ_aux));
quicJ_endpts = quictunneling;
quicV0_endpts = quicdepth;
% figure
% hold on 
% plot(quicJ_aux, quicdepth_er_2, 'DisplayName', 'analytic')
% plot(quicJ, quicdepth_er, 'DisplayName','numeric')
% plot(quicJ_endpts, quicdepth, 'o', 'DisplayName','numeric endpoints')
% legend('location','best')
% xlabel('J_y (Hz)')
% ylabel('V_0 quic (E_r)')

% fit using Taylor series
v0yfcn = a1*(lambertw(-1,-(2*x/a2)^(2/3)/3 * pi^(1/3)))^2;
% T1 = series(v0yfcn, x, 36, 'Order',norder,'Direction','realAxis');
% Teval = real(subs(T1,x,quicJ_endpts));
% figure
% hold on 
% plot(quicJ_aux, quicdepth_er_2, 'DisplayName', 'analytic')
% plot(quicJ, quicdepth_er, '--', 'DisplayName', 'numeric')
% % plot(quicJ_aux, Teval_full, '--', 'DisplayName', 'taylor expansion endpts')
% plot(quicJ_endpts, quicdepth, 'o', 'DisplayName','numeric endpoints')
% plot(quicJ_endpts, Teval, '.', 'DisplayName', 'taylor expansion endpts')
% legend('location','best')
% xlabel('J_y (Hz)')
% ylabel('V_0 quic (E_r)')

%% Try fit 
depth_from_tunnel_old = @(t) -0.70255 + 14.1054 * exp(-t / 0.0000553825) + 10.497 * exp(-t / 0.108124) + 10.6318 * exp(-t / 0.00203487) + 9.01271 * exp(-t / 0.0124361) + 12.449 * exp(-t / 0.000336428);
depth_from_tunnel_str = 'a1 + a2*exp(-x/(1240*a3)) + a4*exp(-x/(1240*a5)) + a6*exp(-x/(1240*a7)) + a8*exp(-x/(1240*a9)) + a10*exp(-x/(1240*a11))';
startvals = [-0.70255, 14.1054, 0.0000553825, 10.497, 0.108124, 10.6318, 0.00203487, 9.01271, 0.0124361, 12.449, 0.000336428];
f = fit(quicJ_aux(2:end)', quicdepth_er_2(2:end)', depth_from_tunnel_str, 'Start', startvals)

%% fit using Cheyshev polynomials
syms s
v0yfcn_aux = @(x) a1 * lambertw( -1, -(quiclim/a2 * (x + 1)).^(2/3) * pi^(1/3)/3 ).^2;
quicDepthEst = zeros(1,length(quicJ_aux));

cy = zeros(1,norder);
quicDepthFactors = sym([]);
for j = 0:norder-1
    for k = 1:norder
        ckj =  real(v0yfcn_aux( cos(pi*(k-1/2)/norder) )) * cos(pi*j*(k-1/2)/norder);
        cy(j+1) = cy(j+1) + ckj;
    end
    cy(j+1) = 2/norder * cy(j+1);
    quicDepthEst = quicDepthEst + cy(j+1)*chebyshevT(j, quicJ_aux/(quiclim/2)-1);
    quicDepthFactors(j+1) = cy(j+1)*chebyshevT(j, s/(quiclim/2)-1);
end
quicDepthEst = quicDepthEst - 1/2 * cy(1);
quicDepthEstFun = @(x) subs(sum(quicDepthFactors) - 1/2 * cy(1), s, x);

figure
tl = tiledlayout('flow','tilespacing','compact');
ax1 = nexttile;
hold on
plot(quicJ_aux, v0yfcn_aux(quicJ_aux/(quiclim/2)-1),'DisplayName', 'analytic')
% plot(quicJ_aux, quicDepthEst, '-', 'DisplayName', 'Cheby approx')
plot(quictunneling, quicdepth,'o', 'DisplayName', 'numeric endpts')
% plot(quicJ_endpts, Teval, '.b', 'DisplayName', 'taylor estimated endpts')
plot(quicJ_endpts, f(quicJ_endpts),'.r', 'DisplayName', 'exp fit estimated endpts')
% plot(quicJ_aux, depth_from_tunnel_old(quicJ_aux/Er), '--', 'DisplayName','old depth from tunnel')
plot(quicJ_aux, f(quicJ_aux), '--r', 'DisplayName','new fit to old depth from tunnel')
ylabel('V_0 quic (E_r)')
legend('location','best')

ax2 = nexttile;
hold on
% plot(quicJ_aux, quicDepthEst-quicdepth_er_2)
plot(quicJ_aux, f(quicJ_aux)-quicdepth_er_2', '-r', 'DisplayName','new fit to old depth from tunnel')
plot(quicJ_endpts, f(quicJ_endpts)-real(jyToDepthFun(quicJ_endpts)), '.r')
yline(0,':')
ylabel('V_0 quic error (E_r)')
% ylim([-2,2])

xlabel(tl, 'J_y (Hz)')
title(tl, ['Up to order ' num2str(norder)])
linkaxes([ax1,ax2],'x')

%% Save polynomial coeffs
% TO DO: export coefficients to txt file, write NI fcn to read them in and construct polynomials

cy = coeffvalues(f)
% save as txt file 
save_coeffs_quic = 1;
if save_coeffs_quic
    fid = fopen('j_to_depth_2d2lattice.txt','w');
    fprintf(fid, '%f\r\n', cy');
    fclose(fid);
end


%% interpolate quad depth 

b1 = 108.4;
b2 = 1.399 * 10^-6;
quadlim = 50;
quadJ_aux = linspace(0, quadlim, 500);

% quadCalibJ = [27.7261+28.2612/2, 40.6189, 50.4666, 56.4366, 61.2801, 62.3651];
% quadCalibV = [2.8000, 2.8800, 2.9400, 2.9800, 3.0200, 3.0500];
quadV_aux = zeros(size(quadJ_aux));
quadV_aux(1) = 0;
tic
for i = 2:length(quadJ_aux)
    quadV_aux(i) = vpasolve( quadJ_aux(i) == b1 * besselj(1, b2*10.^(2.*v)), v, [0, 3.05] );
end
toc
quadJ_endpts = quadtunneling;
quadV_endpts = quadvolts;

%
tic
syms s
jxfcn = @(vx)  b1 * besselj( 1, b2*10.^(2*vx) );
cx = zeros(1,norder);
quadDepthFactors = sym([]);
for j = 0:norder-1
    for k = 1:norder
        jx_k = cos(pi*(k-1/2)/norder);
        v0x_k = vpasolve(jx_k == jxfcn(v)*2/quadlim-1, v, [0,3.05]);
        ckj =  real(v0x_k) * cos(pi*j*(k-1/2)/norder);
        cx(j+1) = cx(j+1) + ckj;
    end
    cx(j+1) = 2/norder * cx(j+1);
    quadDepthFactors(j+1) = cx(j+1)*chebyshevT(j, s/(quadlim/2)-1);
end
quadDepthEstFun = @(x) subs( sum(quadDepthFactors) - 1/2 * cx(1), s, x );
quadDepthEst = quadDepthEstFun(quadJ_aux);
toc

figure
plot(10.^(2*quadV_aux), jxfcn(quadV_aux))

% Plot
figure
tl2 = tiledlayout('flow','tilespacing','compact');

ax1 = nexttile;
hold on
plot(quadJ_aux, quadV_aux,'DisplayName', 'numeric')
plot(quadJ_aux, quadDepthEst, 'DisplayName', 'Cheby approx')
plot(quadJ_endpts, quadV_endpts,'o', 'DisplayName', 'numeric endpts')
plot(quadJ_endpts, quadDepthEstFun(quadJ_endpts),'.r', 'DisplayName', 'cheby estimated endpts')
ylabel('Gauge power quad (V)')
legend('location','best')

ax2 = nexttile;
hold on
plot(quadJ_aux, quadDepthEst-quadV_aux)
plot(quadJ_endpts, quadDepthEstFun(quadJ_endpts)-quadV_endpts, '.r')
yline(0,'--')
ylabel('Gauge power quad error (V)')
ylim([-0.1,0.1])

xlabel(tl2, 'J_x (Hz)')
title(tl2, ['Up to order ' num2str(norder)])
linkaxes([ax1,ax2],'x')
% TO DO: export coefficients to txt file, write NI fcn to read them in and
% construct polynomials


%% Save polynomial coeffs
% TO DO: export coefficients to txt file, write NI fcn to read them in and construct polynomials

cx
% save as txt file 
save_coeffs_quad = 1;
if save_coeffs_quad
    fid = fopen('j_to_v_gaugepower.txt','w');
    fprintf(fid, '%f\r\n', cx');
    fclose(fid);
end


%% convert back to Hz and compare

% %% quic tunneling
% 
% quicdepth_v0 = ramp_amps_full(:,3);
% quicJ = 1229*4/sqrt(pi)*124/100.*quicdepth_er.^(3/4).*exp(-2.*quicdepth_er.^(1/2));
% % quicJ_endpts = 1229*4/sqrt(pi)*124/100.*ramp_amps_convert(:,3).^(3/4).*exp(-2.*ramp_amps_convert(:,3).^(1/2));
% 
% % figure
% % plot(rampt_full, quicJ, 'DisplayName','final')
% % hold on 
% % plot(ramp_time_ms, ramp_amps_hz(:,2), '.--', 'DisplayName','target')
% % % plot(ramp_time_ms, quicJ_endpts, 'o', 'DisplayName','final points')
% % legend('location','best')
% 
% 
% %% quad tunneling
% 
% quaddepth_v = ramp_amps_full(:,2);
% quadJ = 108.4 .* besselj(1, 1.399 .* 10.^(2.*quaddepth_v-6));
% 
% % figure
% % plot(rampt_full, quadJ, 'DisplayName','final')
% % hold on 
% % plot(ramp_time_ms, ramp_amps_hz(:,1), '.--', 'DisplayName','target')
% % plot(ramp_time_ms, quadJ_endpts, 'o', 'DisplayName','final points')
% % legend('location','best')
% 
% figure
% tl = tiledlayout('flow','TileSpacing','compact','Padding','tight');
% 
% ax1 = nexttile;
% plot(rampt_full, quicJ, 'DisplayName','final')
% hold on 
% plot(ramp_time_ms, ramp_amps_hz(:,2), '.--', 'DisplayName','target')
% legend('location','best')
% ylabel('J quic (Hz)')
% 
% ax2 = nexttile;
% plot(rampt_full, quadJ, 'DisplayName','final')
% hold on 
% plot(ramp_time_ms, ramp_amps_hz(:,1), '.--', 'DisplayName','target')
% legend('location','best')
% ylabel('J quad (Hz)')
% 
% xlabel(tl,'time (ms)')
% title(tl,'backward conversions')
% linkaxes([ax1,ax2],'x')