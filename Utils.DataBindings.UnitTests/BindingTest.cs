using NUnit.Framework;
using System;
using Utils.DataBindings;
using Utils.DataBindings.UnitTests;

namespace Tests
{
    public class BindingTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void BindingTest_RightSideShouldUpdateLeftSideBeforeBindingIsCreated()
        {
            string expectedValue = "Testname";
            TestClass left = new TestClass();
            TestClass right = new TestClass();
            right.Name = expectedValue;

            Binding.Create(() => left.Name, () => right.Name);
            Assert.AreEqual(left.Name, expectedValue);
            Assert.AreEqual(right.Name, expectedValue);
        }

        [Test]
        public void BindingTest_EnsureBindingDontMakeUnnecessaryPropertyUpdates()
        {
            string expectedValue = "Testname";
            TestClass left = new TestClass();
            TestClass right = new TestClass();

            Binding.Create(() => left.Name, () => right.Name);
            right.Name = expectedValue;

            Assert.AreEqual(left.Name, expectedValue);
            Assert.AreEqual(right.Name, expectedValue);
            Assert.True(false); // this test is not completed
        }

        [Test]
        public void BindingTest_MultipleRightSideBindingsShouldEmitCorrectValue()
        {
            string value1 = "value1";
            string value2 = "value2";

            TestClass left = new TestClass();
            TestClass right1 = new TestClass();
            TestClass right2 = new TestClass();

            right1.Name = value1;
            right2.Name = value2;

            Binding.Create(() => left.Name, () => right1.Name);
            Binding.Create(() => left.Name, () => right2.Name);

            Assert.AreEqual(left.Name, value2);
            Assert.AreEqual(right1.Name, value2);
            Assert.AreEqual(right2.Name, value2);
        }

        [Test]
        public void BindingTest_MultipleRightSideBindings_UnbindingOneRightSideBindingShouldNotBreakTheRest()
        {
            string value1 = "value1";
            string value2 = "value2";

            TestClass left = new TestClass();
            TestClass right1 = new TestClass();
            TestClass right2 = new TestClass();

            right1.Name = value1;
            right2.Name = value2;

            Binding.Create(() => left.Name, () => right1.Name);
            var binding = Binding.Create(() => left.Name, () => right2.Name);
            binding.Unbind();
            right1.Name = value1;

            Assert.AreEqual(left.Name, value1);
            Assert.AreEqual(right1.Name, value1);
            Assert.AreEqual(right2.Name, value2);
        }

        [Test]
        public void BindingTest_MultipleLeftSideBindingsShouldEmitCorrectValue()
        {
            string value1 = "value1";
            string value2 = "value2";

            TestClass left1 = new TestClass();
            TestClass left2 = new TestClass();
            TestClass right = new TestClass();

            right.Name = value1;

            Binding.Create(() => left1.Name, () => right.Name);
            Binding.Create(() => left2.Name, () => right.Name);

            right.Name = value2;

            Assert.AreEqual(left1.Name, value2);
            Assert.AreEqual(left2.Name, value2);
            Assert.AreEqual(right.Name, value2);
        }

        [Test]
        public void BindingTest_MultipleLeftSideBindings_UnbindingOneLeftSideBindingShouldNotBreakTheRest()
        {
            string value1 = "value1";
            string value2 = "value2";

            TestClass left1 = new TestClass();
            TestClass left2 = new TestClass();
            TestClass right = new TestClass();

            right.Name = value1;

            Binding.Create(() => left1.Name, () => right.Name);
            var binding = Binding.Create(() => left2.Name, () => right.Name);

            binding.Unbind();
            left1.Name = value2;

            Assert.AreEqual(left1.Name, value2);
            Assert.AreEqual(left2.Name, value1);
            Assert.AreEqual(right.Name, value2);
        }

        [Test]
        public void BindingTest_RightSideShouldUpdateLeftSideAfterBindingIsCreated()
        {
            string expectedValue = "Testname";
            TestClass left = new TestClass();
            TestClass right = new TestClass();

            Binding.Create(() => left.Name, () => right.Name);

            right.Name = expectedValue;

            Assert.AreEqual(left.Name, expectedValue);
            Assert.AreEqual(right.Name, expectedValue);
        }

        [Test]
        public void BindingTest_UnbindShouldBreakBindings()
        {
            string value1 = "Testname";
            string value2 = "value2";

            TestClass left = new TestClass();
            TestClass right = new TestClass();
            right.Name = value1;

            var binding = Binding.Create(() => left.Name, () => right.Name);
            binding.Unbind();
            left.Name = value2;

            Assert.AreEqual(left.Name, value2);
            Assert.AreEqual(right.Name, value1);
            Assert.AreNotEqual(left.Name, right.Name);
        }

        [Test]
        public void BindingTest_PropertyPathBindingShouldWork()
        {
            string expectedValue = "Testname";

            TestClass left = new TestClass();
            TestClass right = new TestClass();

            Binding.Create(() => left.Nested.Name, () => right.Nested.Name);

            right.Nested = new TestClass();
            left.Nested = new TestClass();
            right.Nested.Name = expectedValue;

            Assert.AreEqual(left.Nested.Name, expectedValue);
            Assert.AreEqual(right.Nested.Name, expectedValue);
        }

        [Test]
        public void BindingTest_PropertyPath_ShouldNotHaveBindingBetweenPropertyInThePath()
        {
            TestClass left = new TestClass();
            TestClass right = new TestClass();

            right.Nested = new TestClass();
            Binding.Create(() => left.Nested.Name, () => right.Nested.Name);

            left.Nested = new TestClass();

            Assert.AreNotEqual(right.Nested, left.Nested);
        }

        [Test]
        public void BindingTest_LeftNestedPropertyIsNullWhenBindingCreate_ShouldUpdateValueAfterItIsInitialized()
        {
            string value1 = "Testname";

            TestClass left = new TestClass();
            TestClass right = new TestClass();

            right.Nested = new TestClass();
            right.Nested.Name = value1;
            Binding.Create(() => left.Nested.Name, () => right.Nested.Name);

            left.Nested = new TestClass();

            Assert.AreEqual(left.Nested.Name, null);
            Assert.AreEqual(right.Nested.Name, null);
            Assert.AreNotEqual(right.Nested.Name, value1);
        }

        [Test]
        public void BindingTest_UpdaingParentInstanceInPropertyPathShouldBreakTheBindingWithTheOldInstance()
        {
            string value1 = "value1";
            string value2 = "value2";
            string value3 = "value3";

            TestClass left = new TestClass();
            TestClass right = new TestClass();

            right.Nested = new TestClass();
            right.Nested.Name = value1;
            left.Nested = new TestClass();

            Binding.Create(() => left.Nested.Name, () => right.Nested.Name);

            var old = left.Nested;
            left.Nested = new TestClass();
            left.Nested.Name = value3;
            old.Name = value2;

            Assert.AreEqual(left.Nested.Name, value3);
            Assert.AreEqual(right.Nested.Name, value3);
            Assert.AreEqual(old.Name, value2);
        }

        [Test]
        public void BindingTest_LeftNestedPropertyIsNullWhenBindingCreate_ShouldUpdateValueAfterItIsInitialized2()
        {
            string expectedValue = "Testname";

            TestClass left = new TestClass();
            TestClass right = new TestClass();

            right.Nested = new TestClass();

            Binding.Create(() => left.Nested.Name, () => right.Nested.Name);

            left.Nested = new TestClass();
            right.Nested.Name = expectedValue;

            Assert.AreEqual(left.Nested.Name, expectedValue);
            Assert.AreEqual(right.Nested.Name, expectedValue);
        }

        [Test]
        public void BindingTest_Unbind_HoldingAHardReferenceToTheRightSideShouldNotPreventLeftSideToBeGC()
        {
            WeakReference weakReference = null;
            TestClass right = new TestClass();

            new Action(() =>
            {
                string value1 = "Testname";

                TestClass left = new TestClass();
                left.Name = value1;

                var binding = Binding.Create(() => left.Name, () => right.Name);
                binding.Unbind();

                weakReference = new WeakReference(left, true);
            })();

            // left should have gone out of scope about now, 
            // so the garbage collector can clean it up
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsNull(weakReference.Target);
        }

        [Test]
        public void BindingTest_WithoutUnbindIfBothSidesGoOutOfScopeTheyShouldBeGCed()
        {
            WeakReference leftWeakReference = null;
            WeakReference rightWeakReference = null;

            new Action(() =>
            {
                string value1 = "Testname";

                TestClass right = new TestClass();
                TestClass left = new TestClass();

                var binding = Binding.Create(() => left.Name, () => right.Name);
                left.Name = value1;

                leftWeakReference = new WeakReference(left, true);
                rightWeakReference = new WeakReference(right, true);
            })();

            // left should have gone out of scope about now, 
            // so the garbage collector can clean it up
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsNull(leftWeakReference.Target);
            Assert.IsNull(rightWeakReference.Target);
        }

        [Test]
        public void BindingTest_HoldingHardReferenceToIBindingShouldKeepLeftAndRightSidesFromBeingGCed()
        {
            WeakReference leftWeakReference = null;
            WeakReference rightWeakReference = null;

            IBinding binding = null;
            new Action(() =>
            {
                string value1 = "Testname";

                TestClass right = new TestClass();
                TestClass left = new TestClass();

                binding = Binding.Create(() => left.Name, () => right.Name);
                left.Name = value1;

                leftWeakReference = new WeakReference(left, true);
                rightWeakReference = new WeakReference(right, true);
            })();

            // left should have gone out of scope about now, 
            // so the garbage collector can clean it up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            //binding.Unbind();

            Assert.IsNotNull(leftWeakReference.Target);
            Assert.IsNotNull(rightWeakReference.Target);
        }

        [Test]
        public void BindingTest_UnbindShouldAllowLeftAndRightSideToBeGCed()
        {
            WeakReference leftWeakReference = null;
            WeakReference rightWeakReference = null;
            IBinding binding = null;

            new Action(() =>
            {
                string value1 = "Testname";

                TestClass right = new TestClass();
                TestClass left = new TestClass();

                binding = Binding.Create(() => left.Name, () => right.Name);
                left.Name = value1;

                leftWeakReference = new WeakReference(left, true);
                rightWeakReference = new WeakReference(right, true);
            })();

            binding.Unbind();
            // left should have gone out of scope about now, 
            // so the garbage collector can clean it up
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsNull(leftWeakReference.Target);
            Assert.IsNull(rightWeakReference.Target);
        }

        [Test]
        public void BindingTest_WhenBindingReferenceGoesOutOfScopeBothLeftAndRightShouldBeGCed()
        {
            WeakReference leftWeakReference = null;
            WeakReference rightWeakReference = null;

            new Action(() =>
            {
                string value1 = "Testname";

                TestClass right = new TestClass();
                TestClass left = new TestClass();

                var binding = Binding.Create(() => left.Name, () => right.Name);
                left.Name = value1;

                leftWeakReference = new WeakReference(left, true);
                rightWeakReference = new WeakReference(right, true);
            })();

            // left should have gone out of scope about now, 
            // so the garbage collector can clean it up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            //binding.Unbind();

            Assert.IsNull(leftWeakReference.Target);
            Assert.IsNull(rightWeakReference.Target);
        }

        [Test]
        public void BindingTest_TheBindingShouldNotHoldAndReferenceForInstancesInThePropertyPathWhenTheyAreChanged()
        {
            WeakReference weakReference1 = null;
            WeakReference weakReference2 = null;

            new Action(() =>
            {
                string value1 = "Testname";

                TestClass right = new TestClass();
                right.Nested = new TestClass();
                right.Nested.Nested = new TestClass();
                TestClass left = new TestClass();
                left.Nested = new TestClass();
                left.Nested.Nested = new TestClass();

                var binding = Binding.Create(() => left.Nested.Nested.Name, () => right.Nested.Nested.Name);

                weakReference1 = new WeakReference(left.Nested, true);
                weakReference2 = new WeakReference(left.Nested.Nested, true);

                // we are not holding hard reference to left.Nested and left.Nested.Nested they should be GCed
                left.Nested = new TestClass();
                left.Nested.Nested = new TestClass();
                left.Nested.Nested.Name = value1;
            })();

            // left should have gone out of scope about now, 
            // so the garbage collector can clean it up
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsNull(weakReference1.Target);
            Assert.IsNull(weakReference2.Target);
        }

        [Test]
        public void BindingTest_UpdaingParentInstanceInPropertyPathShouldNotHoldHardReferenceToTheOldInstance()
        {
            string value1 = "value1";
            string value2 = "value2";
            string value3 = "value3";
            WeakReference weakReference1 = null;

            TestClass left = new TestClass();
            TestClass right = new TestClass();
            IBinding binding = null;

            new Action(() =>
            {
                right.Nested = new TestClass();
                right.Nested.Name = value1;
                left.Nested = new TestClass();

                binding = Binding.Create(() => left.Nested.Name, () => right.Nested.Name);

                var old = left.Nested;
                left.Nested = new TestClass();
                left.Nested.Name = value3;
                old.Name = value2;

                weakReference1 = new WeakReference(old, true);
            })();

            // left should have gone out of scope about now,
            // so the garbage collector can clean it up
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsNull(weakReference1.Target);
        }
    }
}