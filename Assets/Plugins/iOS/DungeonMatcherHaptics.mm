#import <UIKit/UIKit.h>
#include <atomic>

static std::atomic<unsigned int> dmHapticGeneration(0);

extern "C" void DMCancelHaptic()
{
    ++dmHapticGeneration;
}

extern "C" void DMPlayHaptic(int strength)
{
    unsigned int generation = dmHapticGeneration.load();
    dispatch_async(dispatch_get_main_queue(), ^{
        if (generation != dmHapticGeneration.load()) return;
        if (@available(iOS 10.0, *))
        {
            static UIImpactFeedbackGenerator *light = nil;
            static UIImpactFeedbackGenerator *medium = nil;
            if (light == nil) light = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
            if (medium == nil) medium = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
            UIImpactFeedbackGenerator *generator = strength >= 3 ? medium : light;
            [generator prepare];
            if (@available(iOS 13.0, *))
                [generator impactOccurredWithIntensity:strength == 1 ? 0.35 : 0.7];
            else
                [generator impactOccurred];
        }
    });
}
