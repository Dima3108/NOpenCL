__kernel void test(__global int *output){
    output[get_global_id(0)] = output[get_global_id(0)] * output[get_global_id(0)];
}