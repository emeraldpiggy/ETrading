%% ???????
% ?????0.1?f_change??????y???????????????
syms x;f=x^2;
step=0.1;x=2;k=0;         %????,???,?????
f_change=x^2;             %?????
f_current=x^2;            %???????
ezplot(@(x,f)f-x.^2)       %??????
axis([-2,2,-0.2,3])       %?????
hold on
while f_change>0.000000001                %???????????????????????
    x=x-step*2*x;                         %-2*x???????step???????????
    f_change = f_current - x^2;           %?????????
    f_current = x^2 ;                     %??????????
    plot(x,f_current,'ro','markersize',7) %???????
    drawnow;pause(0.2);
    k=k+1;
end
hold off
fprintf('???%d??????????%e????x??%e\n',k,x^2,x)