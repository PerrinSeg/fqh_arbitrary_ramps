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
ramp_amps_convert(:,1) = quadDepth;


%% quad Gauge beam power (assuming 2D1 lattice depth 5Er)

quadtunneling = ramp_amps(:,1)*J0;
% quadvolts = zeros(size(quadtunneling));
% syms v
% for i = 1:length(quadvolts)
%     jx = quadtunneling(i);
%     if jx == 0
%         val = 0;
%     else
%         val = vpasolve( jx == 108.4 * besselj(1, 1.399 * 10^(2*v-6)), v, [0, 3.05] );
%     end
%     quadvolts(i) = val;
% end
ramp_amps_convert(:,2) = quadtunneling;


%% quic depth

quictunneling = ramp_amps(:,2)*J0;
% quicdepth = zeros(size(quictunneling));
% syms x
% for i = 1:length(quictunneling)
%     jy = quictunneling(i);
%     if jy == 0
%         val = 45;
%     else   
%         val = vpasolve(jy == 1229*4/sqrt(pi)*124/100*x^(3/4)*exp(-2*x^(1/2)), x, [1,Inf]);
%     end
%     quicdepth(i) = val;
% end
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
ramp_duration = [0; ramp_time_ms(2:end)-ramp_time_ms(1:end-1)];


%% save as new file

ramp_new = [ramp_duration, ramp_amps_convert];
ramp_new
% save as txt file 
fid = fopen('ramp_segments.txt','w');
fprintf(fid, '%f %f %f %f %f %f %f\r\n', ramp_new');
fclose(fid);

% ramp_new(:,0) = Delta t (ms)
% ramp_new(:,1) = V0_x (Er)
% ramp_new(:,2) = J_x (Hz)
% ramp_new(:,3) = J_y (Hz)
% ramp_new(:,4) = gauge detuning (Hz)
% ramp_new(:,5) = gradV_x (V)
% ramp_new(:,6) = gradV_y (V)


%% Plot final values

plot_figure = 1;
save_figure = 1;
if plot_figure
    i = 1;

    figure
    tl2 = tiledlayout('flow', "TileSpacing", 'compact', 'Padding', 'compact');
    
    ax(i) = nexttile;
    i = i+1;
    plot(ramp_time_ms, ramp_amps_convert(:,1), '.-', 'DisplayName', 'quad')
    ylabel('Quad lattice depth (E_r)')
 
    ax(i) = nexttile;
    i = i+1;
    hold on
    plot(ramp_time_ms, ramp_amps_convert(:,2), '.-', 'DisplayName', 'quad (gauge power)')
    plot(ramp_time_ms, ramp_amps_convert(:,3), '.-', 'DisplayName', 'quic (lattice depth)')
    ylabel('J (Hz)')
    legend('location','best')

    ax(i) = nexttile;
    plot(ramp_time_ms, ramp_amps_convert(:,4), '.-', 'DisplayName', 'gauge freq')
    ylabel('Gauge freq (Hz)')
    i = i+1;

    ax(i) = nexttile;
    plot(ramp_time_ms, ramp_amps_convert(:,5), '.-', 'DisplayName','quad')
    ylabel('Quad magnetic gradient (V)')
    i = i+1;

    ax(i) = nexttile;
    plot(ramp_time_ms, ramp_amps_convert(:,6), '.-', 'DisplayName','quic')
    ylabel('Quic magnetic gradient (V)')
    i = i+1;

    xlabel(tl2,'time (ms)')
    linkaxes(ax,'x')

    if save_figure
        print('ramp_arb_final_channels','-dpng')
    end
end


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% Interpolate quic depth 

quiclim = 60;
norder = 10;

quicDepthCal = 1.229;
Er = 1240;
a1 = 9/16;
a2 = Er*quicDepthCal;
quicJ_aux = linspace(0,quiclim,200);

% comparing quic analytic to numeric calc
jyfcn = @(x) quicDepthCal .* 4/sqrt(pi) .* Er .* x.^(3/4) .* exp(-2.*x.^(1/2));
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

% Try fit 
depth_from_tunnel_old = @(t) -0.70255 + 14.1054 * exp(-t / 0.0000553825) + 10.497 * exp(-t / 0.108124) + 10.6318 * exp(-t / 0.00203487) + 9.01271 * exp(-t / 0.0124361) + 12.449 * exp(-t / 0.000336428);
depth_from_tunnel_str = 'a1 + a2*exp(-x/(1240*a3)) + a4*exp(-x/(1240*a5)) + a6*exp(-x/(1240*a7)) + a8*exp(-x/(1240*a9)) + b1*exp(-x/(1240*b2))';
startvals = [-0.70255, 14.1054, 0.0000553825, 10.497, 0.108124, 10.6318, 0.00203487, 9.01271, 0.0124361, 12.449, 0.000336428];
fy = fit(quicJ_aux(2:end)', quicdepth_er_2(2:end)', depth_from_tunnel_str, 'Start', startvals)

%% fit quic using Cheyshev polynomials
% syms s
% v0yfcn_aux = @(x) a1 * lambertw( -1, -(quiclim/a2 * (x + 1)).^(2/3) * pi^(1/3)/3 ).^2;
% quicDepthEst = zeros(1,length(quicJ_aux));
% 
% cy = zeros(1,norder);
% quicDepthFactors = sym([]);
% for j = 0:norder-1
%     for k = 1:norder
%         ckj =  real(v0yfcn_aux( cos(pi*(k-1/2)/norder) )) * cos(pi*j*(k-1/2)/norder);
%         cy(j+1) = cy(j+1) + ckj;
%     end
%     cy(j+1) = 2/norder * cy(j+1);
%     quicDepthEst = quicDepthEst + cy(j+1)*chebyshevT(j, quicJ_aux/(quiclim/2)-1);
%     quicDepthFactors(j+1) = cy(j+1)*chebyshevT(j, s/(quiclim/2)-1);
% end
% quicDepthEst = quicDepthEst - 1/2 * cy(1);
% quicDepthEstFun = @(x) subs(sum(quicDepthFactors) - 1/2 * cy(1), s, x);

%% Plot quic
figure
tl = tiledlayout('flow','tilespacing','compact');
ax1 = nexttile;
hold on
plot(quicJ_aux, quicdepth_er_2,'DisplayName', 'analytic')
plot(quicJ_aux, fy(quicJ_aux), '--', 'DisplayName','new fit to old depth from tunnel')
plot(quictunneling, quicdepth,'o', 'DisplayName', 'numeric endpts')
plot(quicJ_endpts, fy(quicJ_endpts),'.', 'DisplayName', 'exp fit estimated endpts')
% plot(quicJ_aux, quicDepthEst, '-', 'DisplayName', 'Cheby approx')
% plot(quicJ_endpts, Teval, '.b', 'DisplayName', 'taylor estimated endpts')
% plot(quicJ_aux, depth_from_tunnel_old(quicJ_aux/Er), '--', 'DisplayName','old depth from tunnel')
ylabel('V_0 quic (E_r)')
legend('location','best')

ax2 = nexttile;
hold on
% plot(quicJ_aux, quicDepthEst-quicdepth_er_2)
plot(quicJ_aux, fy(quicJ_aux)-quicdepth_er_2', '-', 'DisplayName','new fit to old depth from tunnel')
plot(quicJ_endpts, fy(quicJ_endpts)-real(jyToDepthFun(quicJ_endpts)), '.')
yline(0,':')
ylabel('V_0 quic error (E_r)')
% ylim([-2,2])

xlabel(tl, 'J_y (Hz)')
title(tl, 'Exp test quic')
linkaxes([ax1,ax2],'x')

%% Save quic polynomial coeffs
% TO DO: export coefficients to txt file, write NI fcn to read them in and construct polynomials

cy = coeffvalues(fy)
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
% quadlim = 60;
jxfcn = @(vx)  b1 * besselj( 1, b2*10.^(2*vx) );

% quadCalibJ = [27.7261+28.2612/2, 40.6189, 50.4666, 56.4366, 61.2801, 62.3651];
% quadCalibV = [2.8000, 2.8800, 2.9400, 2.9800, 3.0200, 3.0500];
%
% quadJ_aux = linspace(0, quadlim, 500);
% quadV_aux = zeros(size(quadJ_aux));
% quadV_aux(1) = 0;
% tic
% for i = 2:length(quadJ_aux)
%     quadV_aux(i) = vpasolve( quadJ_aux(i) == b1 * besselj(1, b2*10.^(2.*v)), v, [0, 3.05] );
% end
% toc

% quadJ_endpts = quadtunneling;
% quadV_endpts = quadvolts;

quadV_aux = linspace(0,3,200);
quadJ_aux = jxfcn(quadV_aux);
quadlim = quadJ_aux(end);

%%% Try fit 
% depth_from_tunnel_quad_str = 'a1 + a2*exp(-x/(1240*a3)) + a4*exp(-x/(1240*a5)) + a6*exp(-x/(1240*a7)) + a8*exp(-x/(1240*a9)) + b1*exp(-x/(1240*b2))';
% depth_from_tunnel_quad = @(a, x) a(1) + a(2)*exp(-x/(1240*a(3))) + a(4)*exp(-x/(1240*a(5))) + a(6)*exp(-x/(1240*a(7))) + a(8)*exp(-x/(1240*a(9))) + a(10)*exp(-x/(1240*a(11)));
% depth_from_tunnel_quad_str = 'a1 + a2*log(x/(1240*a3)) + a4*log(x/(1240*a5)) + a6*log(x/(1240*a7)) + a8*log(x/(1240*a9)) + b1*log(x/(1240*b2))';
% startvals = [4.938, -2.147, 0.3683, -0.2356, 0.02113, -0.4514, 0.001, 0, 0.0124361, -0.3, 0.0003];
% startvals = [4.938, -2.147, 0.3683, -0.2356, 0.02113, -0.4514, 0.001, 0, 0.0124361];
%
% depth_from_tunnel_quad = @(a, x) a(1) + a(2)*log(x/1240*a(3)) + a(4)*exp(x/1240*a(5)) + a(6)*exp(x/1240*a(7));
% depth_from_tunnel_quad_str = 'a1 + a2*log(x/1240*a3) + a4*exp(x/1240*a5) + a6*exp(x/1240*a7)';
% depth_from_tunnel_quad = @(a, x) a(1) + a(2)*log(x/1240*a(3)) + a(4)*exp(-x/1240/a(5)) + a(6)*exp(-x/1240/a(7));
% depth_from_tunnel_quad_str = 'a1 + a2*log(x/1240*a3) + a4*exp(-x/1240/a5) + a6*exp(-x/1240/a7)';
% startvals = [4, 0.2, 0.1, 0, 1, 0, 1];
% lowerbnd = [-Inf, -Inf, 0, -Inf, -Inf, -Inf, -Inf];
%
% depth_from_tunnel_quad = @(a, x) a(1) + a(2)*exp(-x/1240*a(3)) + a(4)*exp(-x/1240*a(5)) + a(6)*exp(-x/1240*a(7));
% depth_from_tunnel_quad_str = 'a1 + a2*exp(-x/1240*a3) + a4*exp(-x/1240*a5) + a6*exp(-x/1240*a7)';
% startvals = [2.5, -2.6001, 360, 0.3, -10, 0, 1];
% lowerbnd = [-Inf, -Inf, -Inf, -Inf, -Inf, -Inf, -Inf];
% 
% figure
% hold on
% plot(quadJ_aux, depth_from_tunnel_quad(startvals,quadJ_aux), 'DisplayName', 'start values')
% plot(quadJ_aux, quadV_aux, '--', 'DisplayName', 'target')
% xlabel('J_x (Hz)')
% ylabel('gauge voltage (V)')
% title('fit startvals test')
% legend('location','best')
% 
% fx = fit(quadJ_aux(2:end)', quadV_aux(2:end)', depth_from_tunnel_quad_str, 'Start', startvals,'Lower',lowerbnd)
% % fx = fit(quadJ_aux(2:end)', quadV_aux(2:end)', depth_from_tunnel_str, 'Start', startvals)
% % 
% % figure
% % hold on
% % % for i = 1:20
% % %     % plot(quadJ_aux, log(i*quadJ_aux/1240))
% % %     plot(quadJ_aux, i*log(quadJ_aux/1240))
% % % end
% % % plot(quadJ_aux, fx(quadJ_aux),'-')
% % plot(quadJ_aux, quadV_aux,'--')
% 
% %% Plot
% plot_figure = 1;
% if plot_figure
%     quadJ_aux_er = quadJ_aux/1240;
%     quadJ_endpts_er = quadJ_endpts/1240;
% 
%     figure
%     tl2 = tiledlayout('flow','tilespacing','compact');
% 
%     ax1 = nexttile;
%     hold on
%     plot(quadJ_aux_er, fx(quadJ_aux), '-', 'DisplayName', 'exp approx')
%     plot(quadJ_aux_er, quadV_aux, '--', 'DisplayName', 'numeric')
%     plot(quadJ_endpts_er, quadV_endpts,'o', 'DisplayName', 'numeric endpts')
%     plot(quadJ_endpts_er, fx(quadJ_endpts),'.', 'DisplayName', 'exp estimated endpts')
%     ylabel('Gauge voltage quad (V)')
%     legend('location','best')
% 
%     ax2 = nexttile;
%     hold on
%     plot(quadJ_aux_er, fx(quadJ_aux)'-quadV_aux)
%     plot(quadJ_endpts_er, fx(quadJ_endpts)-quadV_endpts, '.')
%     yline(0,'--')
%     ylabel('Gauge power quad error (V)')
%     % ylim([-0.02,0.02])
% 
%     xlabel(tl2, 'J_x (Hz)')
%     title(tl2, 'Exp test quad')
%     linkaxes([ax1,ax2],'x')
% end
% 
% %% chebyshev polynomials quad
% % tic
% % syms s
% % 
% % cx = zeros(1,norder);
% % quadDepthFactors = sym([]);
% % for j = 0:norder-1
% %     for k = 1:norder
% %         jx_k = cos(pi*(k-1/2)/norder);
% %         v0x_k = vpasolve(jx_k == jxfcn(v)*2/quadlim-1, v, [0,3.05]);
% %         ckj =  real(v0x_k) * cos(pi*j*(k-1/2)/norder);
% %         cx(j+1) = cx(j+1) + ckj;
% %     end
% %     cx(j+1) = 2/norder * cx(j+1);
% %     quadDepthFactors(j+1) = cx(j+1)*chebyshevT(j, s/(quadlim/2)-1);
% % end
% % quadDepthEstFun = @(x) subs( sum(quadDepthFactors) - 1/2 * cx(1), s, x );
% % quadDepthEst = quadDepthEstFun(quadJ_aux);
% % toc
% % 
% % % figure
% % % plot(10.^(2*quadV_aux), jxfcn(quadV_aux))
% % 
% % % Plot
% % figure
% % tl2 = tiledlayout('flow','tilespacing','compact');
% % 
% % ax1 = nexttile;
% % hold on
% % plot(quadJ_aux, quadDepthEst, 'DisplayName', 'Cheby approx')
% % plot(quadJ_aux, quadV_aux, '--', 'DisplayName', 'numeric')
% % plot(quadJ_endpts, quadV_endpts,'o', 'DisplayName', 'numeric endpts')
% % plot(quadJ_endpts, quadDepthEstFun(quadJ_endpts),'.r', 'DisplayName', 'cheby estimated endpts')
% % ylabel('Gauge power quad (V)')
% % legend('location','best')
% % 
% % ax2 = nexttile;
% % hold on
% % plot(quadJ_aux, quadDepthEst-quadV_aux)
% % plot(quadJ_endpts, quadDepthEstFun(quadJ_endpts)-quadV_endpts, '.r')
% % yline(0,'--')
% % ylabel('Gauge power quad error (V)')
% % % ylim([-0.1,0.1])
% % 
% % xlabel(tl2, 'J_x (Hz)')
% % title(tl2, ['Cheby quad, up to order ' num2str(norder)])
% % linkaxes([ax1,ax2],'x')
% % % TO DO: export coefficients to txt file, write NI fcn to read them in and
% % % construct polynomials
% 
% 
% % %% Save polynomial coeffs
% % % TO DO: export coefficients to txt file, write NI fcn to read them in and construct polynomials
% % 
% % cx = coeffvalues(fx)
% % % save as txt file 
% % save_coeffs_quad = 1;
% % if save_coeffs_quad
% %     fid = fopen('j_to_v_gaugepower.txt','w');
% %     fprintf(fid, '%f\r\n', cx');
% %     fclose(fid);
% % end
% 


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% 
%% interpolate quad depth 

b1 = 108.4;
b2 = 1.399 * 10^-6;

dcal_quad = 7.4/1.24;
vcal_quad = 3.34;
b2_full = b2*10^(2*vcal_quad)/dcal_quad;

voltToDepthx = @(v) dcal_quad * 10.^(2*(v-vcal_quad));
depthToVoltx = @(d) (1/2)*log10(d/dcal_quad) + vcal_quad;
depthToJx = @(d)  b1 * besselj( 1, b2_full*d );
voltToJx = @(v)  b1 * besselj( 1, b2*10.^(2*v) );

% quadV_aux = zeros(size(quadJ_aux));
% quadV_aux(1) = 0;
% tic
% for i = 2:length(quadJ_aux)
%     quadV_aux(i) = vpasolve( quadJ_aux(i) == b1 * besselj(1, b2*10.^(2.*v)), v, [0, 3.05] );
% end
% toc
quadJ_endpts = quadtunneling;
quadV_endpts = quadvolts;
quadDepth_endpts = voltToDepthx(quadV_endpts);

quadV_aux = linspace(0,3,500);
quadDepth_aux = voltToDepthx(quadV_aux);
% quadDepth_aux = linspace(0, 1.24, 500);
% quadV_aux = depthToVoltx(quadDepth_aux);
quadJ_aux = voltToJx(quadV_aux);

% Try fit 
% depth_from_tunnel_quad_str = 'a1 + a2*exp(-x/(1240*a3)) + a4*exp(-x/(1240*a5)) + a6*exp(-x/(1240*a7)) + a8*exp(-x/(1240*a9)) + b1*exp(-x/(1240*b2))';
% depth_from_tunnel_quad = @(a, x) a(1) + a(2)*exp(-x/(1240*a(3))) + a(4)*exp(-x/(1240*a(5))) + a(6)*exp(-x/(1240*a(7))) + a(8)*exp(-x/(1240*a(9))) + a(10)*exp(-x/(1240*a(11)));
% depth_from_tunnel_quad_str = 'a1 + a2*log(x/(1240*a3)) + a4*log(x/(1240*a5)) + a6*log(x/(1240*a7)) + a8*log(x/(1240*a9)) + b1*log(x/(1240*b2))';
% startvals = [4.938, -2.147, 0.3683, -0.2356, 0.02113, -0.4514, 0.001, 0, 0.0124361, -0.3, 0.0003];
% startvals = [4.938, -2.147, 0.3683, -0.2356, 0.02113, -0.4514, 0.001, 0, 0.0124361];
%
% depth_from_tunnel_quad = @(a, x) a(1) + a(2)*log(x/1240*a(3)) + a(4)*exp(x/1240*a(5)) + a(6)*exp(x/1240*a(7));
% depth_from_tunnel_quad_str = 'a1 + a2*log(x/1240*a3) + a4*exp(x/1240*a5) + a6*exp(x/1240*a7)';
% startvals = [4, 0.2, 0.1, 0, 1, 0, 1];
% lowerbnd = [-Inf, -Inf, 0, -Inf, -Inf, -Inf, -Inf];
%
% depth_from_tunnel_quad2 = @(a, x) a(1) + a(2)*exp(-x/1240*a(3)) + a(4)*exp(-x/1240*a(5)) + a(6)*exp(-x/1240*a(7));
% depth_from_tunnel_quad_str2 = 'a1 + a2*exp(-x/1240*a3) + a4*exp(-x/1240*a5) + a6*exp(-x/1240*a7)';
% startvals2 = [-2000, 8000, -100, 300, -10, 0, 1];
% lowerbnd2 = [-Inf, -Inf, -Inf, -Inf, -Inf, -Inf, -Inf];
%
% depth_from_tunnel_quad2 = @(a, x) a(1) + a(2)*(x/1240) + a(3)*(x/1240).^2 + a(4)*(x/1240).^3;
% depth_from_tunnel_quad_str2 = 'a1 + a2*(x/1240) + a3*(x/1240).^2 + a4*(x/1240)^3';
% startvals2 = [0, 11000, 10, 2]*1240;
% lowerbnd2 = [-Inf, -Inf, -Inf, -Inf];

depth_from_tunnel_quad2 = @(a, x) a(1)*x + a(2)*x.^2 + a(3)*x.^3 + a(4)*x.^4;
depth_from_tunnel_quad_str2 = 'a1*(x/1240) + a2*(x/1240).^2 + a3*(x/1240).^3 + a4*(x/1240).^4';
startvals2 = [0.02, 0, 0.000001,0];
lowerbnd2 = [-Inf, -Inf, -Inf, -Inf];

plot_figure = 0;
if plot_figure
    figure
    hold on
    plot(quadJ_aux, depth_from_tunnel_quad2(startvals2,quadJ_aux))
    plot(quadJ_aux, quadDepth_aux, '--')
    title('fit startvals test')
    xlabel("J_x (Hz)")
    ylabel("lattice depth (E_r)")
end

fx = fit(quadJ_aux', quadDepth_aux', depth_from_tunnel_quad_str2, 'Start', startvals2,'Lower',lowerbnd2)
% fx2 = fit(quadJ_aux', quadDepth_aux', 'poly4')

% Plot
plot_figure = 1;
if plot_figure

    figure
    tl2 = tiledlayout('flow','tilespacing','compact');
    
    ax1 = nexttile;
    hold on
    plot(quadJ_aux, fx(quadJ_aux), '-', 'DisplayName', 'approx')
    plot(quadJ_aux, quadDepth_aux, '--', 'DisplayName', 'numeric')
    plot(quadJ_endpts, quadDepth_endpts,'o', 'DisplayName', 'numeric endpts')
    plot(quadJ_endpts, fx(quadJ_endpts),'.', 'DisplayName', 'estimated endpts')
    ylabel('Gauge depth quad (E_r)')
    legend('location','best')
    
    ax2 = nexttile;
    hold on
    plot(quadJ_aux, fx(quadJ_aux)' - quadDepth_aux)
    plot(quadJ_endpts, fx(quadJ_endpts)-quadDepth_endpts, '.')
    yline(0,'--')
    ylabel('Gauge depth quad error (E_r)')
    % ylim([-0.02,0.02])
    
    xlabel(tl2, 'J_x (Hz)')
    title(tl2, 'polynomial test quad')
    linkaxes([ax1,ax2],'x')
end

% Plot
plot_figure = 1;
if plot_figure
    figure
    tl2 = tiledlayout('flow','tilespacing','compact');
    
    ax1 = nexttile;
    hold on
    plot(quadJ_aux, depthToVoltx(fx(quadJ_aux)), '-', 'DisplayName', 'approx')
    plot(quadJ_aux, depthToVoltx(quadDepth_aux), '--', 'DisplayName', 'numeric')
    % plot(quadJ_aux, quadV_aux, ':', 'DisplayName', 'numeric')
    plot(quadJ_endpts, depthToVoltx(quadDepth_endpts),'o', 'DisplayName', 'numeric endpts')
    plot(quadJ_endpts, depthToVoltx( fx(quadJ_endpts)),'.', 'DisplayName', 'estimated endpts')
    ylabel('Gauge power quad (V)')
    legend('location','best')
    
    ax2 = nexttile;
    hold on
    plot(quadJ_aux, depthToVoltx(fx(quadJ_aux))'-depthToVoltx(quadDepth_aux))
    plot(quadJ_endpts, depthToVoltx(fx(quadJ_endpts))-quadV_endpts, '.')
    yline(0,'--')
    ylabel('Gauge power quad error (V)')
    % ylim([-0.02,0.02])
    
    xlabel(tl2, 'J_x (Hz)')
    title(tl2, 'polynomial test quad')
    linkaxes([ax1,ax2],'x')
end

%% Save polynomial coeffs
% TO DO: export coefficients to txt file, write NI fcn to read them in and construct polynomials

cx = coeffvalues(fx)
% save as txt file 
save_coeffs_quad = 1;
if save_coeffs_quad
    fid = fopen('j_to_v_gaugepower.txt','w');
    fprintf(fid, '%f\r\n', cx');
    fclose(fid);
end

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% convert back to Hz and compare

%% get interpolated ramps
% rampt_full = zeros(ntimes, 100)
ramp_amps_full = [];
rampt_full = [];
dt = 0.5;
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

% quic tunneling
quicJ_full = ramp_amps_full(:,3);
lattice2_calib_volt = 3.56;
lattice2_calib_depth = 72.5;
lattice2_voltage_offset = -0.05;
depthToVolts2 = @(x) lattice2_voltage_offset + lattice2_calib_volt + 1/2 * log10(x/lattice2_calib_depth);
% quicJ = 1229*4/sqrt(pi)*124/100.*quicdepth_er.^(3/4).*exp(-2.*quicdepth_er.^(1/2));
% quicJ_endpts = 1229*4/sqrt(pi)*124/100.*ramp_amps_convert(:,3).^(3/4).*exp(-2.*ramp_amps_convert(:,3).^(1/2));
quic_depth_interp = fy(quicJ_full);

% figure
% plot(rampt_full, quicJ, 'DisplayName','final')
% hold on 
% plot(ramp_time_ms, ramp_amps_hz(:,2), '.--', 'DisplayName','target')
% % plot(ramp_time_ms, quicJ_endpts, 'o', 'DisplayName','final points')
% legend('location','best')

% quad tunneling
quadJ_full = ramp_amps_full(:,2);
quad_depth_interp = fx(quadJ_full);

plot_figure = 1;
if plot_figure
    figure
    tl = tiledlayout('flow','TileSpacing','compact','Padding','tight');
    
    ax1 = nexttile;
    hold on 
    % plot(rampt_full, quicJ_full, 'DisplayName','final old')
    plot(rampt_full, jyfcn(quic_depth_interp), 'DisplayName', 'final')
    plot(ramp_time_ms, ramp_amps_hz(:,2), '.--', 'DisplayName','target')
    plot(ramp_time_ms, jyfcn(fy(quicJ_endpts)), '.r','DisplayName','endpts calculated')
    legend('location','best')
    ylabel('J quic (Hz)')
    
    ax2 = nexttile;
    hold on 
    plot(rampt_full, depthToJx(quad_depth_interp), 'DisplayName','final')
    % plot(ramp_time_ms, depthToJx(fx(ramp_amps_hz(:,1))), 'DisplayName','target+conversion')
    plot(ramp_time_ms, ramp_amps_hz(:,1), '.--', 'DisplayName','target')
    legend('location','best')
    ylabel('J quad (Hz)')
    
    xlabel(tl,'time (ms)')
    title(tl,'backward conversions')
    linkaxes([ax1,ax2],'x')
end

plot_figure = 0;
if plot_figure
    figure
    hold on
    plot(rampt_full, quad_depth_interp, 'DisplayName','final')
    plot(ramp_time_ms, fx(ramp_amps_hz(:,1)), '.--', 'DisplayName','target')
end


%% Another quick test...

gauge_JToDepth_final = @(x) 19.742717 * x + 134.286941 * x.^2 ...
                            + -6152.835178 * x.^3 + 129950.446182 * x.^4;
gauge_DepthToVolts_final = @(x) (1/2)*log10(x/dcal_quad) + vcal_quad;

quic_JToDepth_final = @(x) 4.806925 + 9.845196*exp(-x/0.000188) ...
                           + 10.343631*exp(-x/0.039113) + 6.943367*exp(-x/0.002075) ...
                           + 7.077495*exp(-x/0.007011) + 7.863954*exp(-x/0.000649);
quic_DepthToVolts_final = @(x) lattice2_voltage_offset + lattice2_calib_volt + 1/2 * log10(x/lattice2_calib_depth);

% tunneling final conversions
lattice2D2_ramp_v = quic_DepthToVolts_final( quic_JToDepth_final(quicJ_full/1240) );

gauge1_power_ramp_v = gauge_DepthToVolts_final( gauge_JToDepth_final(quadJ_full/1240) );


% Plot final values
plot_figure = 1;
save_figure = 1;
if plot_figure
    figure
    tl3 = tiledlayout(5,1,"TileSpacing",'compact','Padding','compact');
    
    ax1 = nexttile;
    hold on
    plot(ramp_time_ms, quic_DepthToVolts_final(ramp_amps_convert(:,1)), '.-', 'DisplayName', 'quad')
    plot(rampt_full, lattice2D2_ramp_v, '-', 'DisplayName', 'quic')
    ylabel('Lattice depth (V)')
    legend('location','best')

    ax2 = nexttile;
    plot(rampt_full, gauge1_power_ramp_v, '-', 'DisplayName', 'gauge power')
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

    xlabel(tl3,'time (ms)')
    linkaxes([ax1,ax2,ax3,ax4,ax5],'x')

    if save_figure
        print('ramp_arb_final_ramps','-dpng')
    end
end