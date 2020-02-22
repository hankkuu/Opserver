﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Opserver.Data.HAProxy;
using Xunit;

namespace Opserver.Tests
{
    public class HAProxyTests
    {
        public static string DummyData => @"# pxname,svname,qcur,qmax,scur,smax,slim,stot,bin,bout,dreq,dresp,ereq,econ,eresp,wretr,wredis,status,weight,act,bck,chkfail,chkdown,lastchg,downtime,qlimit,pid,iid,sid,throttle,lbtot,tracked,type,rate,rate_lim,rate_max,check_status,check_code,check_duration,hrsp_1xx,hrsp_2xx,hrsp_3xx,hrsp_4xx,hrsp_5xx,hrsp_other,hanafail,req_rate,req_rate_max,req_tot,cli_abrt,srv_abrt,
so,ny-web01,0,0,2,81,,13395064,9722134016,103474336732,,0,,0,35,0,0,UP,1,1,0,33,20,139857,4719,,1,1,1,,13395064,,2,33,,127,L7OK,200,22,6,12693042,570796,129622,1452,0,0,,,,875,33,
so,ny-web02,0,0,1,92,,13613725,9898075500,112380934783,,0,,0,26,0,0,UP,1,1,0,49,26,139832,4876,,1,1,2,,13613725,,2,24,,116,L7OK,200,50,15,12899818,584998,127771,942,0,0,,,,701,22,
so,ny-web03,0,0,0,90,,12907545,9513697093,98075046208,,0,,0,27,0,0,UP,1,1,0,66,33,139810,5055,,1,1,3,,12907545,,2,39,,114,L7OK,200,33,2,12209805,577319,118436,1848,0,0,,,,468,23,
so,ny-web04,0,0,0,0,,0,0,0,,0,,0,0,0,0,MAINT,1,1,0,0,5,568872,573147,,1,1,4,,0,,2,0,,0,L4TOUT,,2009,0,0,0,0,0,0,0,,,,0,0,
so,ny-web05,0,0,0,109,,13331988,9751192520,107015136128,,0,,0,65,0,0,UP,1,1,0,70,36,61466,5269,,1,1,5,,13331988,,2,37,,114,L7OK,200,1,6,12678125,538466,114295,914,0,0,,,,505,57,
so,ny-web06,0,0,4,179,,13816018,9893089934,113955201607,,0,,0,49,0,0,UP,1,1,0,64,25,139766,5644,,1,1,6,,13816018,,2,45,,114,L7OK,200,28,13,12922897,755519,136392,1027,0,0,,,,427,21,
so,BACKEND,0,0,8,445,0,67093806,48802673512,538256910361,799,0,,0,202,0,0,UP,5,5,0,,16,568886,4569,,1,1,0,,67064340,,1,182,,312,,,,0,63432323,3027098,626533,6234,1605,,,,,2990,156,
others,ny-web07,0,0,5,126,,11373732,6026257934,81660513296,,0,,0,16,1,0,UP,1,1,0,84,45,64543,4977,,1,2,1,,11373731,,2,35,,154,L7OK,200,4,5,9521597,1678084,173163,818,0,0,,,,405,16,
others,ny-web08,0,0,0,135,,13311406,6791105414,92673381133,,0,,0,10,0,0,UP,1,1,0,72,37,11174,4895,,1,2,2,,13311406,,2,39,,150,L7OK,200,5,15,11368655,1730010,211570,1000,0,0,,,,942,10,
others,ny-web09,0,0,0,80,,11436077,5949344053,73395531316,,0,,0,9,0,0,UP,1,1,0,64,37,140005,5179,,1,2,3,,11436077,,2,44,,196,L7OK,200,4,4,9684017,1590611,160353,1040,0,0,,,,355,9,
others,BACKEND,0,0,5,142,0,36122378,18767028205,247729564613,214,0,,0,35,1,0,UP,3,3,0,,12,568882,4411,,1,2,0,,36121214,,1,120,,275,,,,0,30574269,4999637,545088,2865,514,,,,,1720,35,
meta_so,ny-web10,0,0,0,15,,775262,535558317,7726033003,,0,,0,0,0,0,UP,1,1,0,17,18,140376,4426,,1,3,1,,775262,,2,3,,74,L7OK,200,3,1,605667,156938,12614,41,0,0,,,,34,0,
meta_so,ny-web11,0,0,0,16,,645249,430434530,6124899840,,0,,0,0,0,0,UP,1,1,0,8,14,140358,4384,,1,3,2,,645249,,2,0,,43,L7OK,200,3,0,500472,134558,10206,12,0,0,,,,41,0,
meta_so,BACKEND,0,0,0,22,0,1420538,966001362,13850932843,25,0,,0,0,0,0,UP,2,2,0,,3,140376,4275,,1,3,0,,1420511,,1,3,,111,,,,0,1106139,291496,22820,53,30,,,,,77,0,
area51_stackexchange_com,ny-web04,0,0,0,0,,0,0,0,,0,,0,0,0,0,MAINT,1,1,0,0,2,573150,573150,,1,4,1,,0,,2,0,,0,L4TOUT,,2010,0,0,0,0,0,0,0,,,,0,0,
area51_stackexchange_com,ny-web05,0,0,0,23,,2691927,1468461709,7800822527,,0,,0,1,0,0,UP,1,1,0,11,5,61472,4304,,1,4,2,,2691927,,2,5,,56,L7OK,200,44,0,2471617,206095,14074,139,0,0,,,,43,1,
area51_stackexchange_com,ny-web06,0,0,0,11,,2206348,1211876647,6422995592,,0,,0,45,0,0,UP,1,1,0,30,11,571452,160,,1,4,3,,2206348,,2,2,,34,L7OK,200,86,0,2043829,149939,12374,152,0,0,,,,39,1,
area51_stackexchange_com,BACKEND,0,0,0,25,0,4898278,2680342072,14223818119,0,0,,0,46,0,0,UP,2,2,0,,10,571452,101,,1,4,0,,4898275,,1,7,,59,,,,0,4515446,356034,26448,335,15,,,,,85,2,
sstatic,ny-web07,0,0,0,47,,2088592,882428418,7633612482,,0,,0,0,0,0,UP,1,1,0,0,1,634630,4,,1,5,1,,2088592,,2,3,,160,L7OK,200,0,0,1943756,112531,32305,0,0,0,,,,39,0,
sstatic,ny-web08,0,0,0,78,,2278153,961371496,8825922419,,0,,0,0,0,0,UP,1,1,0,3,1,634630,4,,1,5,2,,2278153,,2,4,,164,L7OK,200,0,0,2062964,109206,105983,0,0,0,,,,4269,0,
sstatic,ny-web09,0,0,0,40,,2125861,895635907,7144380838,,0,,0,0,0,0,UP,1,1,0,0,1,634630,4,,1,5,3,,2125861,,2,4,,128,L7OK,200,4,0,1962564,148855,14442,0,0,0,,,,71,0,
sstatic,BACKEND,0,0,0,78,0,6492606,2739435821,23603915739,0,0,,0,0,0,0,UP,3,3,0,,1,634630,4,,1,5,0,,6492606,,1,11,,269,,,,0,5969284,370592,152730,0,0,,,,,4379,0,
stackauth,ny-web04,0,0,0,0,,0,0,0,,0,,0,0,0,0,MAINT,1,1,0,0,2,573145,573145,,1,6,1,,0,,2,0,,0,L4TOUT,,2000,0,0,0,0,0,0,0,,,,0,0,
stackauth,ny-web05,0,0,0,29,,2955309,1389862428,3416003701,,0,,0,0,0,0,UP,1,1,0,16,6,61472,4608,,1,6,2,,2955309,,2,9,,23,L7OK,200,2,127017,2953120,1282,891,16,0,0,,,,32,0,
stackauth,ny-web06,0,0,0,24,,2955354,1389096909,3415077112,,0,,0,0,0,0,UP,1,1,0,27,9,568858,4365,,1,6,3,,2955354,,2,8,,22,L7OK,200,2,127407,2953212,1238,898,6,0,0,,,,35,0,
stackauth,ny-web07,0,0,0,20,,2955354,1389371560,3408611854,,0,,0,0,0,0,UP,1,1,0,5,3,568857,4298,,1,6,4,,2955354,,2,9,,23,L7OK,200,2,127775,2953121,1266,957,10,0,0,,,,30,0,
stackauth,ny-web08,0,0,0,14,,2955357,1389372326,3403837199,,0,,0,0,0,0,UP,1,1,0,6,3,568857,4296,,1,6,5,,2955357,,2,8,,23,L7OK,200,9,127694,2953198,1220,928,11,0,0,,,,28,0,
stackauth,ny-web09,0,0,1,17,,2955357,1389612387,3375754946,,0,,0,0,0,0,UP,1,1,0,31,10,568857,4475,,1,6,6,,2955357,,2,8,,22,L7OK,200,2,127639,2953235,1233,878,10,0,0,,,,34,0,
stackauth,BACKEND,0,0,1,33,0,14776731,6947315610,17019284812,0,0,,0,0,0,0,UP,5,5,0,,1,568859,4285,,1,6,0,,14776731,,1,43,,113,,,,0,14765886,6239,4552,53,0,,,,,159,0,
so_crawler,ny-web01,0,0,0,44,,6698052,2174054825,43895925375,,0,,0,0,0,0,UP,1,1,0,32,20,139857,4772,,1,7,1,,6698052,,2,2,,50,L7OK,200,44,0,4695564,1930978,71415,95,0,0,,,,2,0,
so_crawler,ny-web02,0,0,0,46,,6697856,2174026521,43861853329,,0,,0,0,0,0,UP,1,1,0,48,23,139832,4894,,1,7,2,,6697856,,2,1,,50,L7OK,200,25,0,4695460,1930477,71851,68,0,0,,,,2,0,
so_crawler,ny-web03,0,0,0,41,,6697875,2173796365,43839310527,,0,,0,0,0,0,UP,1,1,0,69,33,139808,5063,,1,7,3,,6697875,,2,2,,50,L7OK,200,34,0,4696169,1929664,71969,73,0,0,,,,0,0,
so_crawler,ny-web04,0,0,0,0,,0,0,0,,0,,0,0,0,0,MAINT,1,1,0,0,3,573140,573140,,1,7,4,,0,,2,0,,0,L4TOUT,,2009,0,0,0,0,0,0,0,,,,0,0,
so_crawler,ny-web05,0,0,0,45,,6698007,2174076306,43860217442,,0,,0,1,0,0,UP,1,1,0,64,34,61466,5306,,1,7,5,,6698007,,2,2,,51,L7OK,200,1,0,4695609,1930813,71512,72,0,0,,,,1,1,
so_crawler,ny-web06,0,0,0,43,,6697920,2173917163,43896255758,,0,,0,0,0,0,UP,1,1,0,66,24,139766,5650,,1,7,6,,6697920,,2,2,,51,L7OK,200,26,0,4696016,1930024,71791,89,0,0,,,,0,0,
so_crawler,BACKEND,0,0,0,82,0,33494082,10871226374,219353562431,4372,0,,0,1,0,0,UP,5,5,0,,14,568854,4582,,1,7,0,,33489710,,1,9,,252,,,,0,23478818,9651956,358538,397,4373,,,,,5,1,
be_careers,ny-web07,0,0,2,130,,8817853,6766467163,7740723145,,0,,0,0,8,0,UP,1,1,0,29,4,79537,4302,,1,8,1,,8817845,,2,35,,73,L7OK,200,11,242,8769418,30023,18325,77,0,0,,,,37,0,
be_careers,ny-web08,0,0,0,162,,12782265,9884135150,9372570840,,0,,0,0,12,0,UP,1,1,0,31,10,71455,4334,,1,8,2,,12782253,,2,14,,75,L7OK,200,10,1,12635744,139628,6808,73,0,0,,,,90,0,
be_careers,ny-web09,0,0,0,115,,9317046,7123346356,9714097962,,0,,0,0,7,0,UP,1,1,0,32,6,156482,4315,,1,8,3,,9317039,,2,16,,101,L7OK,200,1,0,9245987,56752,14016,284,0,0,,,,35,0,
be_careers,BACKEND,0,0,2,343,0,30917187,23773984939,26827448447,0,0,,50,0,27,0,UP,3,3,0,,2,156483,4291,,1,8,0,,30917137,,1,67,,188,,,,0,30651149,226403,39149,484,0,,,,,162,0,
openid,ny-web04,0,0,0,0,,0,0,0,,0,,0,0,0,0,MAINT,1,1,0,0,5,568814,573104,,1,9,1,,0,,2,0,,0,L4TOUT,,2000,0,0,0,0,0,0,0,,,,0,0,
openid,ny-web05,0,0,0,2,,64030,61873426,248855341,,0,,0,0,0,0,UP,1,1,0,16,5,61473,4397,,1,9,2,,64030,,2,0,,7,L7OK,200,263,0,55381,8111,538,0,0,0,,,,7,0,
openid,ny-web06,0,0,0,3,,64030,61760774,250605957,,0,,0,0,0,0,UP,1,1,0,31,9,568845,4436,,1,9,3,,64030,,2,0,,7,L7OK,200,3,0,55330,8159,541,0,0,0,,,,6,0,
openid,BACKEND,0,0,0,3,0,128060,123634200,499461298,0,0,,0,0,0,0,UP,2,2,0,,2,568846,4679,,1,9,0,,128060,,1,0,,14,,,,0,110711,16270,1079,0,0,,,,,13,0,
api_1.1,ny-web04,0,0,0,44,,159,0,0,,0,,0,0,106,53,MAINT,1,1,0,,,,,,1,10,1,,53,,2,0,,6,,,,0,0,0,0,0,0,0,,,,0,0,
api_1.1,ny-web05,0,0,1,30,,1193743,448580826,3816747150,,0,,0,11,1,0,no check,1,1,0,,,,,,1,10,2,,1193742,,2,7,,56,,,,0,1163869,543,29261,57,0,0,,,,1,11,
api_1.1,ny-web06,0,0,0,16,,1193742,448708850,3807938123,,0,,0,0,0,0,no check,1,1,0,,,,,,1,10,3,,1193742,,2,7,,55,,,,0,1163755,564,29369,54,0,0,,,,1,0,
api_1.1,ny-web07,0,0,1,18,,1193742,448915381,3826152298,,0,,0,0,0,0,no check,1,1,0,,,,,,1,10,4,,1193742,,2,7,,55,,,,0,1163786,552,29362,41,0,0,,,,2,0,
api_1.1,ny-web08,0,0,1,16,,1193741,448590254,3808456932,,0,,0,0,0,0,no check,1,1,0,,,,,,1,10,5,,1193741,,2,7,,55,,,,0,1163766,562,29369,43,0,0,,,,0,0,
api_1.1,ny-web09,0,0,2,16,,1193742,448853121,3845131138,,0,,0,0,0,0,no check,1,1,0,,,,,,1,10,6,,1193742,,2,8,,55,,,,0,1163787,578,29329,45,0,0,,,,2,0,
api_1.1,BACKEND,0,0,5,73,0,5968763,2243665984,19104425641,54,0,,0,11,107,53,UP,5,5,0,,1,568841,4289,,1,10,0,,5968762,,1,36,,276,,,,0,5818963,2799,146690,240,66,,,,,6,11,
internal_api,ny-web01,0,0,0,10,,422787,295538544,766353441,,0,,0,0,0,0,no check,1,1,0,,,,,,1,11,1,,422787,,2,1,,17,,,,1841,422668,11,108,0,0,0,,,,4,0,
internal_api,ny-web02,0,0,0,10,,422786,294736396,773129124,,0,,0,0,0,0,no check,1,1,0,,,,,,1,11,2,,422786,,2,1,,17,,,,1844,422687,12,87,0,0,0,,,,5,0,
internal_api,ny-web03,0,0,0,11,,422784,295214846,711277966,,0,,0,0,0,0,no check,1,1,0,,,,,,1,11,3,,422784,,2,1,,17,,,,1875,422684,11,89,0,0,0,,,,7,0,
internal_api,ny-web04,0,0,0,47,,159,0,0,,0,,0,0,106,53,MAINT,1,1,0,,,,,,1,11,4,,53,,2,0,,6,,,,0,0,0,0,0,0,0,,,,0,0,
internal_api,ny-web05,0,0,0,12,,422799,295347213,745097135,,0,,0,0,0,0,no check,1,1,0,,,,,,1,11,5,,422799,,2,1,,17,,,,1812,422688,9,101,1,0,0,,,,6,0,
internal_api,ny-web06,0,0,0,10,,422799,295054768,759123475,,0,,0,0,0,0,no check,1,1,0,,,,,,1,11,6,,422799,,2,1,,18,,,,1892,422696,17,86,0,0,0,,,,4,0,
internal_api,ny-web07,0,0,0,12,,422770,294841925,788084164,,0,,0,0,0,0,no check,1,1,0,,,,,,1,11,7,,422770,,2,1,,18,,,,1879,422665,16,89,0,0,0,,,,4,0,
internal_api,ny-web08,0,0,0,11,,422773,295330999,725192452,,0,,0,0,0,0,no check,1,1,0,,,,,,1,11,8,,422773,,2,1,,18,,,,1825,422657,14,102,0,0,0,,,,5,0,
internal_api,ny-web09,0,0,0,10,,422779,295519933,706504307,,0,,0,0,0,0,no check,1,1,0,,,,,,1,11,9,,422779,,2,1,,18,,,,1829,422690,6,83,0,0,0,,,,7,0,
internal_api,BACKEND,0,0,0,76,0,3382277,2361584624,5974762064,0,0,,0,0,106,53,UP,8,8,0,,1,568833,4291,,1,11,0,,3382330,,1,7,,140,,,,0,3381435,96,745,1,0,,,,,42,0,
be_api,ny-web04,0,0,0,0,,0,0,0,,0,,0,0,0,0,MAINT,1,1,0,0,4,568810,573105,,1,12,1,,0,,2,0,,0,L4TOUT,,2010,0,0,0,0,0,0,0,,,,0,0,
be_api,ny-web05,0,0,0,2,,40734,12771153,63426763,,0,,0,0,0,0,UP,1,1,0,2,20,25740,4439,,1,12,2,,40734,,2,0,,12,L7OK,200,6,3,40214,268,252,0,0,0,,,,0,0,
be_api,ny-web06,0,0,0,2,,40762,12762695,63457939,,0,,0,0,0,0,UP,1,1,0,1,16,25731,4407,,1,12,3,,40762,,2,0,,12,L7OK,200,6,1,40262,268,232,0,0,0,,,,0,0,
be_api,ny-web07,0,0,0,2,,40732,12801136,63090014,,0,,0,0,0,0,UP,1,1,0,0,22,25723,4437,,1,12,4,,40732,,2,0,,9,L7OK,200,4,3,40207,292,233,0,0,0,,,,0,0,
be_api,ny-web08,0,0,0,2,,40726,12713999,62681392,,0,,0,0,0,0,UP,1,1,0,2,22,25714,4432,,1,12,5,,40726,,2,0,,10,L7OK,200,6,2,40193,286,247,0,0,0,,,,0,0,
be_api,ny-web09,0,0,0,2,,40764,12742071,63457020,,0,,0,0,0,0,UP,1,1,0,3,22,25706,4445,,1,12,6,,40764,,2,0,,11,L7OK,200,4,2,40282,276,206,0,0,0,,,,0,0,
be_api,BACKEND,0,0,0,6,0,203718,63791054,316113128,0,0,,0,0,0,0,UP,5,5,0,,2,568809,4309,,1,12,0,,203718,,1,0,,47,,,,0,201158,1390,1170,0,0,,,,,0,0,
dev,ny-web10,0,0,0,7,,5486,1910635,56205377,,0,,0,0,0,0,UP,1,1,0,23,84,11271,1056,,1,13,1,,5486,,2,0,,21,L7OK,302,1,0,4910,542,27,7,0,0,,,,1,0,
dev,ny-web11,0,0,0,7,,10048,4146743,101726439,,0,,0,0,0,0,UP,1,1,0,23,101,11250,823,,1,13,2,,10048,,2,0,,21,L7OK,302,1,1,9076,698,272,2,0,0,,,,26,0,
dev,BACKEND,0,0,0,7,0,15534,6057378,157931816,0,0,,0,0,0,0,UP,2,2,0,,0,634637,0,,1,13,0,,15534,,1,0,,21,,,,0,13986,1240,299,9,0,,,,,27,0,
be_careers_dev,ny-web10,0,0,0,6,,2590,3840770,12175435,,0,,0,0,0,0,UP,1,1,0,95,27,42234,186,,1,14,1,,2590,,2,0,,62,L7OK,200,11,4,1817,735,34,4,0,0,,,,0,0,
be_careers_dev,BACKEND,0,0,0,6,0,2725,4035521,12175435,135,0,,0,0,0,0,UP,1,1,0,,27,42234,186,,1,14,0,,2590,,1,0,,62,,,,0,1817,735,34,4,135,,,,,0,0,
build,ny-web10,0,0,0,6,,897,575762,979450,,0,,0,0,0,0,UP,1,1,0,0,0,634637,0,,1,15,1,,897,,2,0,,30,L4OK,,0,0,148,739,10,0,0,0,,,,0,0,
build,BACKEND,0,0,0,6,0,897,575762,979450,0,0,,0,0,0,0,UP,1,1,0,,0,634637,0,,1,15,0,,897,,1,0,,30,,,,0,148,739,10,0,0,,,,,0,0,
be_wordpress,ny-apache01,0,0,0,63,,167490,86910440,4681042282,,0,,0,0,0,0,UP,1,1,0,165,1,536246,4,,1,16,1,,167490,,2,0,,67,L7OK,200,16,17,132996,27882,6580,32,0,0,,,,382,2,
be_wordpress,ny-apache02,0,0,0,59,,167495,87864855,4718513642,,0,,0,0,0,0,UP,1,1,0,180,0,634637,0,,1,16,2,,167495,,2,0,,67,L7OK,200,3,15,132519,28278,6680,17,0,0,,,,388,1,
be_wordpress,BACKEND,0,0,0,122,0,334986,174775912,9399555924,0,0,,0,0,0,0,UP,2,2,0,,0,634637,0,,1,16,0,,334985,,1,0,,134,,,,0,265515,56160,13261,49,1,,,,,771,3,
be_se_one,ny-web14,0,0,0,76,,785501,463162291,7960025094,,0,,0,2,0,0,UP,1,1,0,30,1,219144,8,,1,17,1,,785501,,2,11,,65,L7OK,200,207,1,743320,21477,20656,46,0,0,,,,51,2,
be_se_one,ny-web15,0,0,0,49,,825471,507165414,19990884063,,0,,0,2,0,0,UP,1,1,0,23,0,634637,0,,1,17,2,,825471,,2,6,,52,L7OK,200,104,1,787018,19583,18836,31,0,0,,,,65,2,
be_se_one,ny-web16,0,0,0,122,,785952,536503911,18256885963,,0,,0,4,0,0,UP,1,1,0,36,3,8929,25,,1,17,3,,785952,,2,1,,89,L7OK,200,76,0,715601,50664,19605,78,0,0,,,,7768,4,
be_se_one,BACKEND,0,0,0,208,0,2396924,1506831616,46207795120,0,0,,0,8,0,0,UP,3,3,0,,0,634637,0,,1,17,0,,2396924,,1,18,,206,,,,0,2245939,91724,59097,155,9,,,,,7884,8,
be_se_one_export,ny-service02,0,0,0,1,,1,754,23621875,,0,,0,0,0,0,UP,1,1,0,1,0,634637,0,,1,18,1,,1,,2,0,,1,L4OK,,0,0,1,0,0,0,0,0,,,,0,0,
be_se_one_export,BACKEND,0,0,0,1,0,1,754,23621875,0,0,,0,0,0,0,UP,1,1,0,,0,634637,0,,1,18,0,,1,,1,0,,1,,,,0,1,0,0,0,0,,,,,0,0,
status,ny-web10,0,0,0,0,,0,0,0,,0,,0,0,0,0,UP,1,1,0,1,1,12380,3,,1,19,1,,0,,2,0,,0,L4OK,,0,0,0,0,0,0,0,0,,,,0,0,
status,ny-web11,0,0,0,6,,10567,9016336,48853662,,0,,0,0,0,0,UP,1,1,0,0,1,12375,4,,1,19,2,,10567,,2,0,,137,L4OK,,0,0,7512,3027,28,0,0,0,,,,0,0,
status,BACKEND,0,0,0,6,0,10567,9016336,48853662,0,0,,0,0,0,0,UP,2,2,0,,0,634637,0,,1,19,0,,10567,,1,0,,137,,,,0,7512,3027,28,0,0,,,,,0,0,
haproxylogs,ny-web10,0,0,0,4,,480,627464,5553354,,0,,0,0,0,0,UP,1,1,0,1,0,634637,0,,1,20,1,,480,,2,0,,12,L4OK,,0,0,396,84,0,0,0,0,,,,0,0,
haproxylogs,BACKEND,0,0,0,4,0,480,627464,5553354,0,0,,0,0,0,0,UP,1,1,0,,0,634637,0,,1,20,0,,480,,1,0,,12,,,,0,396,84,0,0,0,,,,,0,0,
go-away,BACKEND,0,0,0,11,0,1914816,628314451,1447600896,0,0,,1914816,0,0,0,UP,0,0,0,,0,634637,0,,1,21,0,,0,,1,0,,715,,,,0,0,0,0,1914816,0,,,,,95,0,
api_only,BACKEND,0,0,0,22,0,1098346,392215958,687564596,0,0,,1098346,0,0,0,UP,0,0,0,,0,634637,0,,1,22,0,,0,,1,1,,455,,,,0,0,0,0,1098346,0,,,,,0,0,
bad_api,BACKEND,0,0,0,2,0,27550,10599710,31131500,0,0,,27550,0,0,0,UP,0,0,0,,0,634637,0,,1,23,0,,0,,1,0,,10,,,,0,0,0,0,27550,0,,,,,0,0,
no_ssl,BACKEND,0,0,0,1,0,5055,922875,3017835,0,0,,5055,0,0,0,UP,0,0,0,,0,634637,0,,1,24,0,,0,,1,0,,6,,,,0,0,0,0,5055,0,,,,,0,0,
http-in,FRONTEND,,,2733,3213,20000,107360350,90526544046,1086942227543,5464,0,11345649,,,,,OPEN,,,,,,,,,1,25,0,,,,0,272,0,859,,,,0,138492199,18702678,13170599,3007625,62625,,410,996,173435749,,,
fe_careers,FRONTEND,,,766,1015,20000,18398752,23786371894,27527963264,135,0,3467708,,,,,OPEN,,,,,,,,,1,26,0,,,,0,36,0,132,,,,0,30649020,225602,3484961,43535,21910,,75,189,34425030,,,
fe_wordpress,FRONTEND,,,0,136,20000,227966,174896835,9402455818,0,0,14819,,,,,OPEN,,,,,,,,,1,27,0,,,,0,0,0,132,,,,0,265515,56160,28023,49,57,,0,134,349804,,,
processed-ssl-in,FRONTEND,,,0,7,19995,149492,142029239,569682400,0,0,62,,,,,OPEN,,,,,,,,,1,28,0,,,,0,0,0,137,,,,0,122262,20895,1280,5055,0,,0,137,149492,,,
fe_se_one,FRONTEND,,,48,245,19995,1343056,1506987478,46245888779,0,0,73480,,,,,OPEN,,,,,,,,,1,29,0,,,,0,8,0,201,,,,0,2245940,91724,132164,156,421,,18,206,2470405,,,
fe_stackauth,FRONTEND,,,522,666,20000,13861423,6947938790,17983003223,0,0,4852156,,,,,OPEN,,,,,,,,,1,30,0,,,,0,25,0,81,,,,0,14765959,6978,4833785,360,22932,,51,125,19630015,,,";

        [Fact]
        public void HAProxyCSVParsing()
        {
            // TODO: After StackExchange.Utils.Http interception bits are in
        }
    }
}
