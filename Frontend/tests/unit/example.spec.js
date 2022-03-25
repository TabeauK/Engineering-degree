/* eslint-disable no-undef */
import { shallowMount } from "@vue/test-utils";
import Login from "@/views/Login.vue";
import Auth from "@/components/Auth.vue";
import Vuex from "vuex";

describe("Login.vue", () => {
  let store;
  let getters;

  beforeEach(() => {
    getters = {
      isLogged: () => false
    };

    store = new Vuex.Store({
      getters
    });
  });

  it("renders props.msg when passed", () => {
    const msg = "Session expired";
    const wrapper = shallowMount(Login, {
      propsData: { msg },
      store,
    });
    const auth = wrapper.findComponent(Auth);
    expect(auth.props("msg")).toBe(msg);
  });
});
