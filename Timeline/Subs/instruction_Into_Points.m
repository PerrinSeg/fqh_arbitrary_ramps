function [time, values] = instruction_Into_Points(arguments)

    % Analysis depends on the name of the function, here the "first" argument
    name_fun = arguments{1};

    % Use "lower" function to be case-insentitive
    % But keep the name of the function written as usual because it's
    % easier to read...
%     disp(arguments)

    if contains(lower(name_fun), lower("AddPulse")) 
            
        % digitaldata.AddPulse(channel, t_start, t_stop)
        % arguments{1} = AddPulse
        % arguments{2} = channel
        % arguments{3} = t_start
        % arguments{4} = t_stop
        
%         disp("AddPulse")
        t_start = eval(arguments{3});
        t_stop = eval(arguments{4});
        N = 2; % Number of points used for the representation. For digital signal, N = 2 is enough
        time = linspace(t_start, t_stop, N);
        values = 5 * ones([1, N]); % 5 V TTL

    elseif contains(lower(name_fun), lower("DisableClkDist"))
        
        % analogdata.DisableClkDist(t_start, t_stop)
        % arguments{1} = DisableClkDist
        % arguments{2} = t_start
        % arguments{3} = t_stop

%         disp("DisableClkDist")
        t_start = eval(arguments{2});
        t_stop = eval(arguments{3});
        N = 2; % Number of points used for the representation. For digital signal, N = 2 is enough
        time = linspace(t_start, t_stop, N);
        values = 5 * ones([1, N]); % 5 V TTL

    elseif contains(lower(name_fun), lower('AddExpAndRamp'))

        % analogdata.AddExpAndRamp(base_volt, offset_volt, t_start, t_stop, t_ramp, time_const, start_volt, stop_volt, channel)

%         disp('AddExpAndRamp')
        base_volt = eval(arguments{2});
        offset_volt = eval(arguments{3});
        t_start = eval(arguments{4});
        t_stop = eval(arguments{5});
        t_ramp = eval(arguments{6});
        tau = eval(arguments{7});
        start_volt = eval(arguments{8});
        stop_volt = eval(arguments{9});

        N = 100;
        time = linspace(t_start, t_stop, N);
        
%         If nn < (tramp + tstart) Then
%             _data(channel, nn) = y + start_volts + (stop_volts - start_volts) * (nn - tstart) / tramp + offset_volts - base_volts * (2.718281828) ^ (((tstop_msec - tstart_msec) * (nn - tstart) / (tstop - tstart)) / timeconst_msec)
%         Else
%             _data(channel, nn) = y + stop_volts + offset_volts - base_volts * (2.718281828) ^ (((tstop_msec - tstart_msec) * (nn - tstart) / (tstop - tstart)) / timeconst_msec)
        ExpAndRampFun = @(t, base_volt_0, offset_volt_0, t_start_0, t_stop_0, t_ramp_0, tau_0, start_volt_0, stop_volt_0) ...
                                (t < (t_ramp_0 + t_start_0)) .* ( start_volt_0 + (stop_volt_0 - start_volt_0) * (t - t_start_0) / t_ramp_0 ...
                                     + offset_volt_0 - base_volt_0 * exp((t - t_start_0) / tau_0)   ) ...
                                     + (t >= (t_ramp_0 + t_start_0)) .* ( stop_volt_0 ...
                                     + offset_volt_0 - base_volt_0 * exp((t - t_start_0) / tau_0)   );

        values = ExpAndRampFun(time, base_volt, offset_volt, t_start, t_stop, t_ramp, tau, start_volt, stop_volt);


    elseif contains(lower(name_fun), lower('AddExp'))

        % analogdata.AddExp(base_volt, offset_volt, t_start, t_stop, time_const, channel)
        
%         disp('AddExp')
        base_volt = eval(arguments{2});
        offset_volt = eval(arguments{3});
        t_start = eval(arguments{4});
        t_stop = eval(arguments{5});
        tau = eval(arguments{6});

        N = 100;
        time = linspace(t_start, t_stop, N);
        
        % y + offset_volts - base_volts * (2.718281828) ^ (((tstop_msec - tstart_msec) * (nn - tstart) / (tstop - tstart)) / timeconst_msec)
        ExpFun = @(t, base_volt_0, offset_volt_0, t_start_0, t_stop_0, tau_0) ...
                    offset_volt_0 - base_volt_0 * exp((t - t_start_0) / tau_0);

        values = ExpFun(time, base_volt, offset_volt, t_start, t_stop, tau);


    elseif contains(lower(name_fun), lower('AddRampLogSine3PhaseNoReturn'))

%     _declspec (dllexport) void insert_ramp_log_sine3_phase_return(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3,
% 	    double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
% 	    double phase_pi_start, double phase_pi_stop, double phase_ramp_dur,
% 	    double t_start, double ramp_dur, double hold_dur,
% 	    double voltage_offset, double calib_volt, double calib_depth,
% 	    int dat_chan, uInt32 *NI_waveform) {

        lattice_depth = eval(arguments{2});
        amp1 = eval(arguments{3});
        amp2 = eval(arguments{4});
        amp3 = eval(arguments{5});
        freq1 = eval(arguments{6});
        freq2 = eval(arguments{7});
        freq3 = eval(arguments{8});
        phase_start_pi = eval(arguments{9});
        phase_stop_pi = eval(arguments{10});
        phase_ramp_dur = eval(arguments{11});
        t_start = eval(arguments{12});
        t_ramp_dur = eval(arguments{13});
        t_hold_dur = eval(arguments{14});
        voltage_offset = eval(arguments{15});
        calib_volt = eval(arguments{16});
        calib_depth = eval(arguments{17});

        N = 10000;
        time = linspace(t_start, t_start + t_ramp_dur + t_hold_dur + phase_ramp_dur, N);

%         disp(time)

        Sine3RampPhaseNoReturnFun = @(t, lattice_depth_0, amp1_0, amp2_0, amp3_0, freq1_0, freq2_0, freq3_0, phase_start_pi_0, phase_stop_pi_0, phase_ramp_dur_0, t_start_0, t_ramp_0, t_hold_0) ...
                      lattice_depth_0 * ( 1 + ( (t <= t_start_0 + t_ramp_0) .* (t - t_start_0) / t_ramp_0 + (t > t_start_0 + t_ramp_0) ) ...
                      .* ( amp1_0 * sin( 2*pi * freq1_0 * (t-t_start_0)/1000 ) ...
                       + amp2_0 * sin( 2*pi * freq2_0 * (t-t_start_0)/1000 ) ...
                       + amp3_0 * sin( 2*pi * freq3_0 * (t-t_start_0)/1000 ...
                       + pi * ( (t <= t_start_0 + t_ramp_0 + t_hold_0) .* phase_start_pi_0 ...
                       + (t > t_start_0 + t_ramp_0 + t_hold_0) .* ( phase_start_pi_0 + (t - t_start_0 - t_ramp_0 - t_hold_0) / phase_ramp_dur_0 * (phase_stop_pi_0 - phase_start_pi_0) ) ) ) ) );

        values_lin = Sine3RampPhaseNoReturnFun(time, lattice_depth, amp1, amp2, amp3, freq1, freq2, freq3, phase_start_pi, phase_stop_pi, phase_ramp_dur, t_start, t_ramp_dur, t_hold_dur);
        values = voltage_offset + calib_volt + 0.5 * (log10(values_lin) - log10(calib_depth));


    elseif contains(lower(name_fun), lower('AddRampLogSine3PhaseReturn'))

%     _declspec (dllexport) void insert_ramp_log_sine3_phase_return(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3,
% 	    double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
% 	    double phase_pi_start, double phase_pi_stop, double phase_ramp_dur,
% 	    double t_start, double ramp_dur, double hold_dur,
% 	    double voltage_offset, double calib_volt, double calib_depth,
% 	    int dat_chan, uInt32 *NI_waveform) {

        lattice_depth = eval(arguments{2});
        amp1 = eval(arguments{3});
        amp2 = eval(arguments{4});
        amp3 = eval(arguments{5});
        freq1 = eval(arguments{6});
        freq2 = eval(arguments{7});
        freq3 = eval(arguments{8});
        phase_start_pi = eval(arguments{9});
        phase_stop_pi = eval(arguments{10});
        phase_ramp_dur = eval(arguments{11});
        t_start = eval(arguments{12});
        t_ramp_dur = eval(arguments{13});
        t_hold_dur = eval(arguments{14});
        voltage_offset = eval(arguments{15});
        calib_volt = eval(arguments{16});
        calib_depth = eval(arguments{17});

        N = 10000;
        time = linspace(t_start, t_start + 2*t_ramp_dur + 2*t_hold_dur + 2*phase_ramp_dur, N);

%         disp(time)

Sine3RampPhaseReturnFun = @(t, lattice_depth_0, amp1_0, amp2_0, amp3_0, freq1_0, freq2_0, freq3_0, phase_start_pi_0, phase_stop_pi_0, phase_ramp_dur_0, t_start_0, t_ramp_0, t_hold_0) ...
              lattice_depth_0 * ( 1 + (  (t <= t_start_0 + t_ramp_0) .* (t - t_start_0) / t_ramp_0 + (t > t_start_0 + t_ramp_0) .* (t < t_start_0 + t_ramp_0 + 2*t_hold_0 + 2*phase_ramp_dur_0) ...
              + (t >= t_start_0 + t_ramp_0 + 2*t_hold_0 + 2*phase_ramp_dur_0) .* ( 1 - (t - (t_start_0 + t_ramp_0 + 2*t_hold_0 + 2*phase_ramp_dur_0)) / t_ramp_0 ) ) ...
              .* ( amp1_0 * sin( 2*pi * freq1_0 * (t-t_start_0)/1000 ) ...
               + amp2_0 * sin( 2*pi * freq2_0 * (t-t_start_0)/1000 ) ...
               + amp3_0 * sin( 2*pi * freq3_0 * (t-t_start_0)/1000 ...
               + pi * ( (t <= t_start_0 + t_ramp_0 + t_hold_0) .* phase_start_pi_0 ...
               + (t > t_start_0 + t_ramp_0 + t_hold_0) .* (t < t_start_0 + t_ramp_0 + t_hold_0 + phase_ramp_dur_0) .* ( phase_start_pi_0 + (t - t_start_0 - t_ramp_0 - t_hold_0) / phase_ramp_dur_0 * (phase_stop_pi_0 - phase_start_pi_0) ) ...
               + (t > t_start_0 + t_ramp_0 + t_hold_0 + phase_ramp_dur_0) .* (t < t_start_0 + t_ramp_0 + t_hold_0 + 2*phase_ramp_dur_0) .* ( phase_stop_pi_0 + (t - t_start_0 - t_ramp_0 - t_hold_0 - phase_ramp_dur_0) / phase_ramp_dur_0 * (phase_start_pi_0 - phase_stop_pi_0) ) ...
               + (t > t_start_0 + t_ramp_0 + t_hold_0 + 2*phase_ramp_dur_0) .* phase_start_pi_0 ) ) ) );

        values_lin = Sine3RampPhaseReturnFun(time, lattice_depth, amp1, amp2, amp3, freq1, freq2, freq3, phase_start_pi, phase_stop_pi, phase_ramp_dur, t_start, t_ramp_dur, t_hold_dur);
        values = voltage_offset + calib_volt + 0.5 * (log10(values_lin) - log10(calib_depth));



    elseif contains(lower(name_fun), lower('AddRampLogSine3Return'))
    
    % _declspec (dllexport) void insert_ramp_log_sine3(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3,
    % 			double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
    % 			double phase_pi, double t_start, double ramp_dur, double hold_dur,
    % 			double voltage_offset, double calib_volt, double calib_depth,
    % 			int dat_chan, uInt32 *NI_waveform) {
    % 	/* Adds sinusoidal modulation with 3 frequency components and phase offset between two components.
    % 	*
    % 	* Arguments:
    % 	* lattice_depth:       lattice depth in recoils
    % 	* rel_amp:             relative amplitude in % lattice depth of sinusoidal modulation of tunneling
    % 	* freq_Hz:             frequency in Hz
    % 	* phase_pi				phase offset between two frequency components
    % 	* t_start:             Leading edge time, in milliseconds from the analog output start trigger
    % 	* tstop:               Trailing edge time, in milliseconds from the analog output start trigger
    % 	* voltage_offset:		voltage difference between PD value on panel and scope
    % 	* calib_volt:			voltage from Kapitza-Dirac (in ExpConstants.txt)
    % 	* calib_depth:			lattice depth corresponding to calib_volt from Kapitza-Dirac (in ExpConstants.txt)
    % 	* dat_chan:             (0-7) Specifies the channel to which the ramp waveform is added */

%         disp('AddRampLogSine3')
        lattice_depth = eval(arguments{2});
        amp1 = eval(arguments{3});
        amp2 = eval(arguments{4});
        amp3 = eval(arguments{5});
        freq1 = eval(arguments{6});
        freq2 = eval(arguments{7});
        freq3 = eval(arguments{8});
        phase_pi = eval(arguments{9});
        t_start = eval(arguments{10});
        t_ramp_dur = eval(arguments{11});
        t_hold_dur = eval(arguments{12});
        voltage_offset = eval(arguments{13});
        calib_volt = eval(arguments{14});
        calib_depth = eval(arguments{15});

        N = 10000;
        time = linspace(t_start, t_start + t_ramp_dur + t_hold_dur + t_ramp_dur, N);

%         disp(time)


        Sine3RampReturnFun = @(t, lattice_depth_0, amp1_0, amp2_0, amp3_0, freq1_0, freq2_0, freq3_0, phase_pi_0, t_start_0, t_ramp_0, t_hold_0) ...
                      lattice_depth_0 * (1 + (  (t <= t_start_0 + t_ramp_0) .* (t - t_start_0) / t_ramp_0 + (t > t_start_0 + t_ramp_0) .* (t < t_start_0 + t_ramp_0 + t_hold_0) ...
                      + (t >= t_start_0 + t_ramp_0 + t_hold_0) .* ( 1 - (t - (t_start_0 + t_ramp_0 + t_hold_0)) / t_ramp_0 ) ) .* ( amp1_0 * sin( 2*pi * freq1_0 * (t-t_start_0)/1000 ) ...
                       + amp2_0 * sin( 2*pi * freq2_0 * (t-t_start_0)/1000 ) ...
                       + amp3_0 * sin( 2*pi * freq3_0 * (t-t_start_0)/1000 + pi*phase_pi_0) ) );

        values_lin = Sine3RampReturnFun(time, lattice_depth, amp1, amp2, amp3, freq1, freq2, freq3, phase_pi, t_start, t_ramp_dur, t_hold_dur);
        values = voltage_offset + calib_volt + 0.5 * (log10(values_lin) - log10(calib_depth));


    elseif contains(lower(name_fun), lower('AddRampLogSine3NoReturn'))
    
    % _declspec (dllexport) void insert_ramp_log_sine3_return(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3,
    % 			double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
    % 			double phase_pi, double t_start, double ramp_dur, double hold_dur,
    % 			double voltage_offset, double calib_volt, double calib_depth,
    % 			int dat_chan, uInt32 *NI_waveform) {
    % 	/* Adds sinusoidal modulation with 3 frequency components and phase offset between two components.
    % 	*
    % 	* Arguments:
    % 	* lattice_depth:       lattice depth in recoils
    % 	* rel_amp:             relative amplitude in % lattice depth of sinusoidal modulation of tunneling
    % 	* freq_Hz:             frequency in Hz
    % 	* phase_pi				phase offset between two frequency components
    % 	* t_start:             Leading edge time, in milliseconds from the analog output start trigger
    % 	* tstop:               Trailing edge time, in milliseconds from the analog output start trigger
    % 	* voltage_offset:		voltage difference between PD value on panel and scope
    % 	* calib_volt:			voltage from Kapitza-Dirac (in ExpConstants.txt)
    % 	* calib_depth:			lattice depth corresponding to calib_volt from Kapitza-Dirac (in ExpConstants.txt)
    % 	* dat_chan:             (0-7) Specifies the channel to which the ramp waveform is added */

%         disp('AddRampLogSine3')
        lattice_depth = eval(arguments{2});
        amp1 = eval(arguments{3});
        amp2 = eval(arguments{4});
        amp3 = eval(arguments{5});
        freq1 = eval(arguments{6});
        freq2 = eval(arguments{7});
        freq3 = eval(arguments{8});
        phase_pi = eval(arguments{9});
        t_start = eval(arguments{10});
        t_ramp_dur = eval(arguments{11});
        t_hold_dur = eval(arguments{12});
        voltage_offset = eval(arguments{13});
        calib_volt = eval(arguments{14});
        calib_depth = eval(arguments{15});

        N = 10000;
        time = linspace(t_start, t_start + t_ramp_dur + t_hold_dur, N);
        
        Sine3RampFun = @(t, lattice_depth_0, amp1_0, amp2_0, amp3_0, freq1_0, freq2_0, freq3_0, phase_pi_0, t_start_0, t_ramp_0, t_hold_0) ...
                        lattice_depth_0 * ( 1 + ( (t <= t_start_0 + t_ramp_0) .* (t - t_start_0) / t_ramp_0 + (t > t_start_0 + t_ramp_0) ) .* ( amp1_0 * sin( 2*pi * freq1_0 * (t-t_start_0)/1000 ) ...
                       + amp2_0 * sin( 2*pi * freq2_0 * (t-t_start_0)/1000 ) ...
                       + amp3_0 * sin( 2*pi * freq3_0 * (t-t_start_0)/1000 + pi*phase_pi_0) ) );

        values_lin = Sine3RampFun(time, lattice_depth, amp1, amp2, amp3, freq1, freq2, freq3, phase_pi, t_start, t_ramp_dur, t_hold_dur);
        values = voltage_offset + calib_volt + 0.5 * (log10(values_lin) - log10(calib_depth));


    elseif contains(lower(name_fun), lower('AddRampLogSine3'))

%         _declspec (dllexport) void insert_ramp_log_sine3(double lattice_depth, double rel_amp_1, double rel_amp_2, double rel_amp_3,
% 	        double freq_Hz_1, double freq_Hz_2, double freq_Hz_3,
% 	        double phase_pi, double tunneling_init, double tunneling_final, double detuning_Hz_init, double detuning_Hz_final,
% 	        double t_start, double t_stop, double t_ref,
% 	        double voltage_offset, double calib_volt, double calib_depth,
% 	        int dat_chan, uInt32 *NI_waveform) 

        lattice_depth = eval(arguments{2});
        amp1 = eval(arguments{3});
        amp2 = eval(arguments{4});
        amp3 = eval(arguments{5});
        freq1 = eval(arguments{6});
        freq2 = eval(arguments{7});
        freq3 = eval(arguments{8});
        phase_pi = eval(arguments{9});
        tunneling_init = eval(arguments{10});
        tunneling_final = eval(arguments{11});
        detuning_Hz_init = eval(arguments{12});
        detuning_Hz_final = eval(arguments{13});
        t_start = eval(arguments{14});
        t_stop = eval(arguments{15});
        t_ref = eval(arguments{16});
        voltage_offset = eval(arguments{17});
        calib_volt = eval(arguments{18});
        calib_depth = eval(arguments{19});

        N = 10000;
        time = linspace(t_start, t_stop, N);
        
        RampLogSine3Fun = @(t, lattice_depth_0, amp1_0, amp2_0, amp3_0, freq1_0, freq2_0, freq3_0, phase_pi_0, tunneling_init_0, tunneling_final_0, detuning_Hz_init_0, detuning_Hz_final_0, t_start_0, t_stop_0, t_ref_0) ...
                       lattice_depth_0 * (1 + (tunneling_init_0 + (tunneling_final_0 - tunneling_init_0) .* (t - t_start_0) / (t_stop_0 - t_start_0) ) .* ( amp1_0 * sin( 2*pi * freq1_0 .* (t-t_start_0 + t_ref_0)/1000 ) ...
                       + amp2_0 * sin( 2*pi * (freq2_0 + (detuning_Hz_init_0 + 1/2*(detuning_Hz_final_0 - detuning_Hz_init_0) .* (t - t_start_0) / (t_stop_0 - t_start_0) ) ) .* (t-t_start_0 + t_ref_0)/1000 ) ...
                       + amp3_0 * sin( 2*pi * (freq3_0 - (detuning_Hz_init_0 + 1/2*(detuning_Hz_final_0 - detuning_Hz_init_0) .* (t - t_start_0) / (t_stop_0 - t_start_0) ) ) .* (t-t_start_0 + t_ref_0)/1000 + pi*phase_pi_0) ) );

        values_lin = RampLogSine3Fun(time, lattice_depth, amp1, amp2, amp3, freq1, freq2, freq3, phase_pi, tunneling_init, tunneling_final, detuning_Hz_init, detuning_Hz_final, t_start, t_stop, t_ref);
        values = voltage_offset + calib_volt + 0.5 * (log10(values_lin) - log10(calib_depth));

        

    elseif contains(lower(name_fun), lower('AddRamp'))

        % analogdata.AddRamp(start_volt, stop_volt, t_start, t_stop, channel)
        
%         disp('AddRamp')
        start_volt = eval(arguments{2});
        stop_volt = eval(arguments{3});
        t_start = eval(arguments{4});
        t_stop = eval(arguments{5});

        N = 2;
        time = linspace(t_start, t_stop, N);
        
        % start_volts + (stop_volts - start_volts) * (nn - tstart) / (tstop - tstart)
        RampFun = @(t, start_volt_0, stop_volt_0, t_start_0, t_stop_0) ...
                    start_volt_0 + (stop_volt_0 - start_volt_0) * (t - t_start_0) / (t_stop_0 - t_start_0);

        values = RampFun(time, start_volt, stop_volt, t_start, t_stop);



    elseif contains(lower(name_fun), lower('AddSmoothRamp'))
        
        % analogdata.AddSmoothRamp(start_volt, end_volt, t_start, t_stop, channel)

%         disp('AddSmoothRamp')
        start_volt = eval(arguments{2});
        stop_volt = eval(arguments{3});
        t_start = eval(arguments{4});
        t_stop = eval(arguments{5});

        N = 100;
        time = linspace(t_start, t_stop, N);
        
        % x = (nn - tstart) / (tstop - tstart)
        % f = 10 * Math.Pow(x, 3) - 15 * Math.Pow(x, 4) + 6 * Math.Pow(x, 5)
        % _data(channel, nn) = start_volts + (stop_volts - start_volts) * f
        SmoothRampFun = @(t, start_volt_0, stop_volt_0, t_start_0, t_stop_0) ...
                            start_volt_0 + (stop_volt_0 - start_volt_0) * ( 10 * ( (t - t_start_0) / (t_stop_0 - t_start_0) ).^3 ...
                            - 15 * ( (t - t_start_0) / (t_stop_0 - t_start_0) ).^4  + 6 * ( (t - t_start_0) / (t_stop_0 - t_start_0) ).^5 );

        values = SmoothRampFun(time, start_volt, stop_volt, t_start, t_stop);


    elseif contains(lower(name_fun), lower('AddStep'))

        % analogdata2.AddStep(step_volt, t_start, t_end, channel)
        
%         disp('AddStep')
        step_volt = eval(arguments{2});
        t_start = eval(arguments{3});
        t_stop = eval(arguments{4});
        N = 2; % Number of points used for the representation. For digital signal, N = 2 is enough
        time = linspace(t_start, t_stop, N);
        values = step_volt * ones([1, N]);


    elseif contains(lower(name_fun), lower('AddFromBinaryTransportFile'))

        % analogdata.AddFromBinaryTransportFile(filename, t_start, t_stop, channel)
        % Z:\Temp\RbExpSoftware\NI_653x_routines\ni653x
        
%         disp('AddFromBinaryTransportFile')
        filename = eval(arguments{2});
        t_start = eval(arguments{3});
        t_stop = eval(arguments{4});

        filename = strtrim(split(filename, '+'));
        for i = 1:numel(filename)
            filename{i} = replace(filename{i}, '"', '');
        end
        filename = [filename{:}];

        % File name in lower letters...
        filename = replace(filename, 'c:\\users\\greinerlab\\documents\\', 'z:\\temp\\'); 
        filename = replace(filename, "_12.5mhz.bin", ".txt");
        values = importdata(filename);
        N = numel(values);
        time = linspace(t_start, t_stop, N);
        
        % Extract the voltage analog control sent to each power supply
        
        % According to "Z:\Temp\RbExpSoftware\ExpControl\mathematica\interpolate_transport_12.5MHz"
        % "file_text" contains the total current (in A) to be sent by the two power
        % supplies, mounted in parallel which form ps1 (or ps2, ps3, ps4). So the
        % current of each power supply is
        values = values ./ 2;

        % Still according to "Z:\Temp\RbExpSoftware\ExpControl\mathematica\interpolate_transport_12.5MHz"
        % The current is actually 1.5 higher during Part 2 of the transport
        if contains(filename, 'part2')
            values = 1.5 .* values;
        end
       
        % Now for getting volage, the analog voltage control is from 0-5 V
        % to go from zero current to max current 
        % Max current = 60 A for ps1 and ps2
        if contains(filename, '1hr') || contains(filename, '2hr')
            values = 5 * values ./ 60;
        else
        % Max current = 100 A for ps3 and ps4
            values = 5 .* values ./ 100;
        end
        values = values'; % Need to take the transpose


    elseif contains(lower(name_fun), lower('AddLogSine3Phase'))
   
%         disp('AddLogSine3Phase')
        lattice_depth = eval(arguments{2});
        amp1 = eval(arguments{3});
        amp2 = eval(arguments{4});
        amp3 = eval(arguments{5});
        freq1 = eval(arguments{6});
        freq2 = eval(arguments{7});
        freq3 = eval(arguments{8});
        phase_pi = eval(arguments{9});
        t_start = eval(arguments{10});
        t_stop = eval(arguments{11});
        t_ref = eval(arguments{12});
        voltage_offset = eval(arguments{13});
        calib_volt = eval(arguments{14});
        calib_depth = eval(arguments{15});

        N = 10000;
        time = linspace(t_start, t_stop, N);

       
        Sine3Fun = @(t, lattice_depth_0, amp1_0, amp2_0, amp3_0, freq1_0, freq2_0, freq3_0, phase_pi_0, t_start_0, t_stop_0, t_ref_0) ...
                       lattice_depth_0 * (1 + amp1_0 * sin( 2*pi * freq1_0 * (t-t_start_0+t_ref_0)/1000 ) ...
                       + amp2_0 * sin( 2*pi * freq2_0 * (t-t_start_0+t_ref_0)/1000 ) ...
                       + amp3_0 * sin( 2*pi * freq3_0 * (t-t_start_0+t_ref_0)/1000 + pi*phase_pi_0));

        values_lin = Sine3Fun(time, lattice_depth, amp1, amp2, amp3, freq1, freq2, freq3, phase_pi, t_start, t_stop, t_ref);
        values = voltage_offset + calib_volt + 0.5 * (log10(values_lin) - log10(calib_depth));



    elseif contains(lower(name_fun), lower('AddLogSine3'))
    
        % analogdata.AddSine3(lattice1_low_volt, latticemod_amp_1, latticemod_amp_2,
        % latticemod_amp_3, mod_freq_kHz_1, mod_freq_kHz_2, mod_freq_kHz_3, stat_phase_pi, 
        % mod_start_time, mod_end_time, lattice2D765_power) '3-component modulation
        % Freq are given in Hz
    
%         disp('AddSine3')
%         offset_volt = eval(arguments{2});
%         amp1 = eval(arguments{3});
%         amp2 = eval(arguments{4});
%         amp3 = eval(arguments{5});
%         freq1 = eval(arguments{6});
%         freq2 = eval(arguments{7});
%         freq3 = eval(arguments{8});
%         phase_pi = eval(arguments{9});
%         t_start = eval(arguments{10});
%         t_stop = eval(arguments{11});
% 
%         N = 200;
%         time = linspace(t_start, t_stop, N);
% 
%         Sine3Fun = @(t, offset_volt_0, amp1_0, amp2_0, amp3_0, freq1_0, freq2_0, freq3_0, phase_pi_0, t_start_0, t_stop_0) ...
%                         offset_volt_0 + amp1_0 * sin( 2*pi * freq1_0*1000 * t/1000 ) ...
%                         + amp2_0 * sin( 2*pi * freq2_0*1000 * t/1000 ) ...
%                         + amp3_0 * sin( 2*pi * freq3_0*1000 * t/1000 + pi*phase_pi_0);
% 
%         values = Sine3Fun(time, offset_volt, amp1, amp2, amp3, freq1, freq2, freq3, phase_pi, t_start, t_stop);

        % analogdata.AddSine3(lattice_depth, rel_amp_1, rel_amp_2, rel_amp_3, freq_kHz_1, freq_kHz_2, freq_kHz_3, 
        % phase_pi, start_time, end_time,
        % voltage_offset, calib_volt, calib_depth, lattice2D765_power) '3-component modulation
        % Freq are converted from kHz to Hz below

%         disp('AddLogSine3')
        lattice_depth = eval(arguments{2});
        amp1 = eval(arguments{3});
        amp2 = eval(arguments{4});
        amp3 = eval(arguments{5});
        freq1 = eval(arguments{6});
        freq2 = eval(arguments{7});
        freq3 = eval(arguments{8});
        phase_pi = eval(arguments{9});
        t_start = eval(arguments{10});
        t_stop = eval(arguments{11});
        voltage_offset = eval(arguments{12});
        calib_volt = eval(arguments{13});
        calib_depth = eval(arguments{14});

        N = 10000;
        time = linspace(t_start, t_stop, N);

       
        Sine3Fun = @(t, lattice_depth_0, amp1_0, amp2_0, amp3_0, freq1_0, freq2_0, freq3_0, phase_pi_0, t_start_0, t_stop_0) ...
                       lattice_depth_0 * (1 + amp1_0 * sin( 2*pi * freq1_0 * (t-t_start_0)/1000 ) ...
                       + amp2_0 * sin( 2*pi * freq2_0 * (t-t_start_0)/1000 ) ...
                       + amp3_0 * sin( 2*pi * freq3_0 * (t-t_start_0)/1000 + pi*phase_pi_0));

        values_lin = Sine3Fun(time, lattice_depth, amp1, amp2, amp3, freq1, freq2, freq3, phase_pi, t_start, t_stop);
        values = voltage_offset + calib_volt + 0.5 * (log10(values_lin) - log10(calib_depth));

    elseif contains(lower(name_fun), lower('AddSineRamp'))
    
        % analogdata2.AddSineRamp(offset_volt, ampl_volt, freq_start, freq_stop, t_start, t_stop, channel)
        % Freq are given in Hz
    
%         disp('AddSineRamp')
        offset_volt = eval(arguments{2});
        ampl_volt = eval(arguments{3});
        freq_start = eval(arguments{4});
        freq_stop = eval(arguments{5});
        t_start = eval(arguments{6});
        t_stop = eval(arguments{7});

        N = 5000;
        time = linspace(t_start, t_stop, N);

%         periodI = 1.0 / freq_Hz_start
%         Dim FiT As Double = (1.0 / periodI) * (nn - tstart)
%         Dim PhiT = 2.0 * Math.PI * FiT * (1.0 + FiT * (freq_Hz_stop / freq_Hz_start - 1) / (2.0 * freq_Hz_start * (tstop_msec - tstart_msec) / 1000.0))
%         Dim y As Double = offset_volts + amp_volts * Math.Sin(PhiT)
        SineRampFun = @(t, offset_volt_0, ampl_volt_0, freq_start_0, freq_stop_0, t_start_0, t_stop_0) ...
                        offset_volt_0 + ampl_volt_0 * sin( 2*pi * freq_start_0 * t/1000 ... 
                        .* (1 + freq_start_0 * (t - t_start_0) / 1000 * ( freq_stop_0 / freq_start_0 - 1 ) / ( freq_start_0 * 2 * (t_stop_0 - t_start_0) / 1000 )  ) );

        values = SineRampFun(time, offset_volt, ampl_volt, freq_start, freq_stop, t_start, t_stop);

             
    elseif contains(lower(name_fun), lower('AddLogRamp'))

        % analogdata.AddLogRamp(start_volt, end_volt, t_start, t_stop, channel)   
       
%         disp('AddLogRamp')
        start_volt = eval(arguments{2});
        end_volt = eval(arguments{3});
        t_start = eval(arguments{4});
        t_stop = eval(arguments{5});

        N = 100;
        time = linspace(t_start, t_stop, N);

        % _data(channel, nn) = start_volts + 0.5 * Math.Log10(1 + (nn - tstart) / (tstop - tstart) * (Math.Pow(10, 2 * stop_volts - 2 * start_volts) - 1))
        LogRampFun = @(t, start_volt_0, stop_volt_0, t_start_0, t_stop_0) ...
                        start_volt_0 + 0.5 * log10( 1 + (t - t_start_0) / (t_stop_0 - t_start_0) * (10.^(2 * (stop_volt_0 - start_volt_0) ) - 1) );

        values = LogRampFun(time, start_volt, end_volt, t_start, t_stop);
            
        
    elseif contains(lower(name_fun), lower('AddInterpolatedRampUsingFile'))
        
        % analogdata.AddInterpolatedRampUsingFile(ramp_path, volt_ini, volt_end, t_start, t_end, channel)

%         disp('AddInterpolatedRampUsingFile')
        filename = eval(arguments{2});
        volt_ini = eval(arguments{3});
        volt_end = eval(arguments{4});
        t_start = eval(arguments{5});
        t_stop = eval(arguments{6});

        filename = strtrim(split(filename, '+'));
        for i = 1:numel(filename)
            filename{i} = replace(filename{i}, '"', '');
        end
        filename = [filename{:}];
        file_txt = importdata(filename);
        
        if t_start < t_stop
            time = t_start + (t_stop - t_start) .* file_txt(:,1);
            time = time';
            % values = file_txt(:,2)';
            values = (file_txt(:,2)-file_txt(1,2))/(file_txt(end,2)-file_txt(1,2)) * (volt_end-volt_ini) + volt_ini;
            values = values';
        else
            time = [];
            values = [];
        end

    end
end